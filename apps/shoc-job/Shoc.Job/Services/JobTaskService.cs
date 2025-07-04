using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Shoc.Core;
using Shoc.Job.Data;
using Shoc.Job.K8s;
using Shoc.Job.Model;
using Shoc.Job.Model.Job;
using Shoc.Job.Model.JobTask;

namespace Shoc.Job.Services;

/// <summary>
/// The job task service
/// </summary>
public class JobTaskService : JobServiceBase
{
    /// <summary>
    /// Creates new instance of the job service
    /// </summary>
    /// <param name="jobRepository">The job repository</param>
    /// <param name="validationService">The validation service</param>
    /// <param name="jobProtectionProvider">The protection provider</param>
    /// <param name="jobClientFactory">The job client factory</param>
    /// <param name="taskClientFactory">The task client factory</param>
    /// <param name="taskRepository">The task repository</param>
    public JobTaskService(IJobRepository jobRepository, JobValidationService validationService, JobProtectionProvider jobProtectionProvider, KubernetesJobClientFactory jobClientFactory, KubernetesTaskClientFactory taskClientFactory, IJobTaskRepository taskRepository) 
        : base(jobRepository, validationService, jobProtectionProvider, jobClientFactory, taskClientFactory, taskRepository)
    {
    }
    
    /// <summary>
    /// Gets all the objects
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<JobTaskModel>> GetAll(string workspaceId, string jobId)
    {
        // require the parent object
        await this.RequireById(workspaceId, jobId);

        // get from the storage
        return await this.taskRepository.GetAll(workspaceId, jobId);
    }
    
    /// <summary>
    /// Gets all extended objects
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<JobTaskExtendedModel>> GetAllExtended(string workspaceId, string jobId)
    {
        // require the parent object
        await this.RequireById(workspaceId, jobId);

        // get from the storage
        return await this.taskRepository.GetAllExtended(workspaceId, jobId);
    }

    /// <summary>
    /// Gets the object by id
    /// </summary>
    /// <returns></returns>
    public async Task<JobTaskModel> GetById(string workspaceId, string jobId, string id)
    {
        // require the parent object
        await this.RequireById(workspaceId, jobId);
        
        // try load the object
        var result = await this.taskRepository.GetById(workspaceId, jobId, id);

        // check if object exists
        if (result == null)
        {
            throw ErrorDefinition.NotFound().AsException();
        }
        
        // return result
        return result;
    }
    
    /// <summary>
    /// Gets the object by sequence
    /// </summary>
    /// <returns></returns>
    public async Task<JobTaskModel> GetBySequence(string workspaceId, string jobId, long sequence)
    {
        // require the parent object
        await this.RequireById(workspaceId, jobId);
        
        // try load the object
        var result = await this.taskRepository.GetBySequence(workspaceId, jobId, sequence);

        // check if object exists
        if (result == null)
        {
            throw ErrorDefinition.NotFound().AsException();
        }
        
        // return result
        return result;
    }
        
    /// <summary>
    /// Gets the extended object by id
    /// </summary>
    /// <returns></returns>
    public async Task<JobTaskExtendedModel> GetExtendedById(string workspaceId, string jobId, string id)
    {
        // require the parent object
        await this.RequireById(workspaceId, jobId);

        // try load the object
        var result = await this.taskRepository.GetExtendedById(workspaceId, jobId, id);

        // check if object exists
        if (result == null)
        {
            throw ErrorDefinition.NotFound().AsException();
        }
        
        // return result
        return result;
    }
    
    /// <summary>
    /// Gets the extended object by sequence
    /// </summary>
    /// <returns></returns>
    public async Task<JobTaskExtendedModel> GetExtendedBySequence(string workspaceId, string jobId, long sequence)
    {
        // require the parent object
        await this.RequireById(workspaceId, jobId);

        // try load the object
        var result = await this.taskRepository.GetExtendedBySequence(workspaceId, jobId, sequence);

        // check if object exists
        if (result == null)
        {
            throw ErrorDefinition.NotFound().AsException();
        }
        
        // return result
        return result;
    }
    
    /// <summary>
    /// Gets the logs of the task
    /// </summary>
    /// <returns></returns>
    public async Task<Stream> GetLogsById(string workspaceId, string jobId, string id)
    {
        // require the parent object
        var job = await this.RequireById(workspaceId, jobId);

        // require the task to exist 
        var task = await this.GetById(workspaceId, jobId, id);

        // gets the logs of the task
        return await this.GetTaskLogs(job, task);
    }
    
    /// <summary>
    /// Gets the logs of the task by sequence
    /// </summary>
    /// <returns></returns>
    public async Task<Stream> GetLogsBySequence(string workspaceId, string jobId, long sequence)
    {
        // require the parent object
        var job = await this.RequireById(workspaceId, jobId);

        // require the task to exist 
        var task = await this.GetBySequence(workspaceId, jobId, sequence);

        // gets the logs of the task
        return await this.GetTaskLogs(job, task);
    }

    /// <summary>
    /// Gets the task logs by given job and task
    /// </summary>
    /// <param name="job">The job</param>
    /// <param name="task">The task</param>
    /// <returns></returns>
    private async Task<Stream> GetTaskLogs(JobModel job, JobTaskModel task)
    {
        // created or pending tasks cannot have logs
        if (task.Status is JobTaskStatuses.CREATED or JobTaskStatuses.PENDING)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_STATUS, "The task in Created or Pending status does not have any logs").AsException();
        }

        // the task did not run yet
        if (!task.RunningAt.HasValue)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_STATUS, "The task did not run").AsException();
        }

        // protection provider
        var protector = this.jobProtectionProvider.Create(job.WorkspaceId);
        
        // the cluster configuration
        var clusterConfig = protector.Unprotect(job.ClusterConfigEncrypted);

        // get the client
        using var client = this.taskClientFactory.Create(clusterConfig, task.Type);

        // get the task logs
        return await client.GetTaskLogs(job, task);
    }
}