using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Quartz;
using Shoc.Job.Data;
using Shoc.Job.K8s;
using Shoc.Job.K8s.Model;
using Shoc.Job.Model.Job;
using Shoc.Job.Model.JobTask;
using Shoc.Job.Services;

namespace Shoc.Job.Quartz;

/// <summary>
/// A quartz job to sync Kubernetes and Shoc job and task statuses 
/// </summary>
public class KubernetesWatchQuartzJob : IJob
{
    /// <summary>
    /// The terminal statuses for the task
    /// </summary>
    private static readonly ISet<string> TASK_TERMINAL_STATUSES = new HashSet<string> {JobTaskStatuses.SUCCEEDED, JobTaskStatuses.FAILED, JobTaskStatuses.CANCELLED };
    
    /// <summary>
    /// The job repository
    /// </summary>
    private readonly IJobRepository jobRepository;

    /// <summary>
    /// The task repository
    /// </summary>
    private readonly IJobTaskRepository taskRepository;

    /// <summary>
    /// The task status repository
    /// </summary>
    private readonly IJobTaskStatusRepository taskStatusRepository;
    
    /// <summary>
    /// The task client factory for Kubernetes
    /// </summary>
    private readonly KubernetesTaskClientFactory taskClientFactory;
    
    /// <summary>
    /// The protection provider
    /// </summary>
    private readonly JobProtectionProvider jobProtectionProvider;

    /// <summary>
    /// The scheduler factory
    /// </summary>
    private readonly ISchedulerFactory schedulerFactory;

    /// <summary>
    /// Creates new instance of watch job
    /// </summary>
    /// <param name="jobRepository">The job repository</param>
    /// <param name="taskRepository">The task repository</param>
    /// <param name="taskStatusRepository">The task status repository</param>
    /// <param name="taskClientFactory">The task client factory for Kubernetes</param>
    /// <param name="jobProtectionProvider">The protection provider</param>
    /// <param name="schedulerFactory">The scheduler factory</param>
    public KubernetesWatchQuartzJob(IJobRepository jobRepository, IJobTaskRepository taskRepository, IJobTaskStatusRepository taskStatusRepository, KubernetesTaskClientFactory taskClientFactory, JobProtectionProvider jobProtectionProvider, ISchedulerFactory schedulerFactory)
    {
        this.jobRepository = jobRepository;
        this.taskRepository = taskRepository;
        this.taskStatusRepository = taskStatusRepository;
        this.taskClientFactory = taskClientFactory;
        this.jobProtectionProvider = jobProtectionProvider;
        this.schedulerFactory = schedulerFactory;
    }

    /// <summary>
    /// Builds a job key for the given job type
    /// </summary>
    /// <param name="jobId">The job id</param>
    /// <param name="taskId">The task id</param>
    /// <returns></returns>
    public static JobKey BuildKey(string jobId, string taskId)
    {
        return new JobKey($"{KubernetesWatchConstants.WATCH_JOB_PREFIX}-{taskId}", jobId);
    }
    
    /// <summary>
    /// Sync the statuses from Kubernetes with Shoc objects
    /// </summary>
    /// <param name="context">The execution context</param>
    /// <returns></returns>
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            // execute the job logic
            await this.ExecuteImpl(context);
        }
        catch (Exception e)
        {
            // should fire again
            var refireImmediately = e is not FatalQuartzException;
            
            // fire the trigger again immediately
            throw new JobExecutionException(e, refireImmediately);
        }
    }

    /// <summary>
    /// Implementation of job execution
    /// </summary>
    /// <param name="context">The execution context</param>
    /// <returns></returns>
    private async Task ExecuteImpl(IJobExecutionContext context)
    {
        // the data
        var data = context.MergedJobDataMap;

        // the workspace id
        var workspaceId = data.GetValueOrDefault(KubernetesWatchConstants.WORKSPACE_ID) as string;

        // the job id
        var jobId = data.GetValueOrDefault(KubernetesWatchConstants.JOB_ID) as string;

        // the task id
        var taskId = data.GetValueOrDefault(KubernetesWatchConstants.TASK_ID) as string;

        // the trigger index
        var triggerIndexStr = data.GetValueOrDefault(KubernetesWatchConstants.TRIGGER_INDEX) as string;
        
        // require the job 
        var job = await this.jobRepository.GetById(workspaceId, jobId);

        // ensure job exist to continue
        if (job == null)
        {
            return;
        }
        
        // ensure task id is given
        if (string.IsNullOrWhiteSpace(taskId))
        {
            await this.jobRepository.FailById(workspaceId, jobId, new JobFailInputModel
            {
                WorkspaceId = workspaceId,
                Id = jobId,
                Message = "The identifier of the task is missing. Job is in invalid state.",
                CompletedAt = DateTime.UtcNow
            });
            return;
        }
        
        // require the task
        var task = await this.taskRepository.GetById(workspaceId, jobId, taskId);
        
        // ensure task exist to continue
        if (task == null)
        {
            await this.jobRepository.FailById(workspaceId, jobId, new JobFailInputModel
            {
                WorkspaceId = workspaceId,
                Id = jobId,
                Message = $"The task {taskId} is missing",
                CompletedAt = DateTime.UtcNow
            });
            return;
        }

        // the task is completed or is in terminal state do not continue
        if (task.CompletedAt.HasValue || TASK_TERMINAL_STATUSES.Contains(task.Status))
        {
            return;
        }
        
        // ensure trigger index is available
        if (!int.TryParse(triggerIndexStr, out var triggerIndex))
        {
            await this.taskStatusRepository.CompleteTaskById(workspaceId, jobId, taskId, new JobTaskCompleteInputModel
            {
                WorkspaceId = workspaceId,
                JobId = jobId,
                Id = taskId,
                Status = JobTaskStatuses.FAILED,
                Message = "The job task is in invalid state: trigger index is missing or not valid.",
                CompletedAt = DateTime.UtcNow
            });
            return;
        }
        
        // the protector
        var protector = this.jobProtectionProvider.Create();

        // create a kubernetes client for the task
        using var client = this.taskClientFactory.Create(protector.Unprotect(job.ClusterConfigEncrypted), task.Type);

        // get the task result
        var taskResult = await client.GetTaskStatus(job, task);
        
        // the state is not OK for some unknown reason
        if (taskResult.ObjectState != K8sObjectState.OK)
        {
            // determine the message
            var message = taskResult.ObjectState switch
            {
                K8sObjectState.NOT_FOUND => "The target task object is not found in the cluster",
                K8sObjectState.DUPLICATE_OBJECT => "The target task has a duplicate object in the cluster",
                _ => "The task object is in invalid state in the cluster"
            };
            
            await this.taskStatusRepository.CompleteTaskById(workspaceId, jobId, taskId, new JobTaskCompleteInputModel
            {
                WorkspaceId = workspaceId,
                JobId = jobId,
                Id = taskId,
                Status = JobTaskStatuses.FAILED,
                Message = message,
                CompletedAt = DateTime.UtcNow
            });
            return;
        }

        // the task in the cluster has a start time so should be reported as running
        if (taskResult.StartTime.HasValue && task.Status == JobTaskStatuses.PENDING)
        {
            await this.taskStatusRepository.RunningTaskById(workspaceId, jobId, taskId, new JobTaskRunningInputModel
            {
                WorkspaceId = workspaceId,
                JobId = jobId,
                Id = taskId,
                RunningAt = taskResult.StartTime.Value,
                Message = "The task executed started"
            });
        }
        
        // if task object in the cluster is in terminal state 
        if (taskResult.CompletionTime.HasValue)
        {
            await this.taskStatusRepository.CompleteTaskById(workspaceId, jobId, taskId, new JobTaskCompleteInputModel
            {
                WorkspaceId = workspaceId,
                JobId = jobId,
                Id = taskId,
                Status = taskResult.Succeeded ? JobTaskStatuses.SUCCEEDED : JobTaskStatuses.FAILED,
                Message = "The task execution completed",
                CompletedAt = taskResult.CompletionTime.Value 
            });
            return;
        }
        
        // schedule next trigger to check again
        await this.ScheduleNextTrigger(context, triggerIndex);
    }

    /// <summary>
    /// Schedule another trigger for later
    /// </summary>
    /// <param name="context">The execution context</param>
    /// <param name="triggerIndex">The trigger index</param>
    /// <returns></returns>
    private async Task ScheduleNextTrigger(IJobExecutionContext context, int triggerIndex)
    {
        // next trigger data
        var nextTriggerData = new JobDataMap();
        nextTriggerData.PutAll(context.Trigger.JobDataMap);
        nextTriggerData.Put(KubernetesWatchConstants.TRIGGER_INDEX, (triggerIndex + 1).ToString());

        // get next delay based on the current trigger index
        var nextDelay = triggerIndex < KubernetesWatchConstants.NEXT_TRIGGERS.Length
            ? KubernetesWatchConstants.NEXT_TRIGGERS[triggerIndex]
            : KubernetesWatchConstants.NEXT_TRIGGERS.Last();
        
        // build the next trigger
        var nextTrigger = context.Trigger
            .GetTriggerBuilder()
            .ForJob(context.JobDetail)
            .UsingJobData(nextTriggerData)
            .StartAt(DateTimeOffset.UtcNow.AddSeconds(nextDelay))
            .Build();

        // get the scheduler
        var scheduler = await this.schedulerFactory.GetScheduler();

        // reschedule the same job with the new trigger
        await scheduler.RescheduleJob(context.Trigger.Key, nextTrigger);
    }
}