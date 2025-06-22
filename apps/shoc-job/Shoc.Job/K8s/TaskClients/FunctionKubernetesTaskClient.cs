using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using Shoc.Core;
using Shoc.Job.K8s.Model;
using Shoc.Job.Model;
using Shoc.Job.Model.Job;
using Shoc.Job.Model.JobTask;

namespace Shoc.Job.K8s.TaskClients;

/// <summary>
/// The function Kubernetes task client
/// </summary>
public class FunctionKubernetesTaskClient : BaseKubernetesTaskClient
{
    /// <summary>
    /// Creates new instance of Function task client
    /// </summary>
    /// <param name="config">The cluster configuration</param>
    public FunctionKubernetesTaskClient(string config) : base(config, JobTaskTypes.FUNCTION)
    {
    }

    /// <summary>
    /// Checks if cluster supports the given task type
    /// </summary>
    /// <returns></returns>
    public override Task<bool> IsSupported()
    {
        return Task.FromResult(true);
    }

    /// <summary>
    /// Submits the task with given input to the cluster
    /// </summary>
    /// <param name="input">The task input</param>
    /// <returns></returns>
    public override async Task<InitTaskResult> Submit(InitTaskInput input)
    {
        // the instance name
        var instanceName = GetInstanceName(input.Task);
        
        // the input arguments when given
        var extraArgs = input.Args?.Args ?? [];
        
        // the default labels
        var labels = CreateManagedLabels(new ManagedMetadata
        {
            Name = instanceName,
            Component = ShocK8sComponents.TASK,
            PartOf = ShocK8sComponents.JOB,
            WorkspaceId = input.Task.WorkspaceId,
            JobId = input.Task.JobId,
            TaskId = input.Task.Id
        });
        
        // build the object to submit
        var instance = new V1Job
        {
            ApiVersion = "batch/v1",
            Kind = "Job",
            Metadata = new V1ObjectMeta
            {
                Name = instanceName,
                NamespaceProperty = input.Namespace,
                Labels = labels
            },
            Spec = new V1JobSpec
            {
                BackoffLimit = 0, 
                Template = new V1PodTemplateSpec
                {
                    Metadata = new V1ObjectMeta
                    {
                        Labels = new Dictionary<string, string>(labels)
                        {
                            [ShocK8sLabels.SHOC_POD_ROLE] = ShocK8sPodRoles.EXECUTOR
                        }
                    },
                    Spec = new V1PodSpec
                    {
                        RestartPolicy = DEFAULT_RESTART_POLICY,
                        ImagePullSecrets = GetPullSecrets(input),
                        Containers = new List<V1Container>
                        {
                            new()
                            {
                                EnvFrom = GetEnvSources(input),
                                Name = GetMainContainerName(input.Task),
                                Image = input.PullSecret.Image,
                                Env = GetIndexerVars(input.Task),
                                Resources = new V1ResourceRequirements
                                {
                                    Requests = GetTaskResources(input.Task),
                                    Limits = GetTaskResources(input.Task)
                                },
                                SecurityContext = GetSecurityContext(input.Runtime),
                                Args = extraArgs.Length > 0 ? extraArgs : null
                            }
                        },
                        ServiceAccountName = input.ServiceAccount,
                        SecurityContext = GetPodSecurityContext(input.Runtime)
                    }
                }
            }
        };

        // submit the object to the cluster
        await this.client.CreateNamespacedJobAsync(instance, input.Namespace);
        
        return new InitTaskResult();
    }
    
    /// <summary>
    /// Gets the task status in the cluster
    /// </summary>
    /// <param name="job">The job instance</param>
    /// <param name="task">The task instance</param>
    /// <returns></returns>
    public override async Task<TaskK8sStatusResult> GetTaskStatus(JobModel job, JobTaskModel task)
    {
        // get the target job
        var batchJobs = await this.GetKubernetesJobs(job, task);

        // the target object is not found
        if (batchJobs == null || batchJobs.Count == 0)
        {
            return new TaskK8sStatusResult
            {
                ObjectState = K8sObjectState.NOT_FOUND
            };
        }
        
        // a duplicate object is detected
        if (batchJobs.Count > 1)
        {
            return new TaskK8sStatusResult
            {
                ObjectState = K8sObjectState.DUPLICATE_OBJECT
            };
        }

        // the first object
        var batchJob = batchJobs.First();

        // get executor pods
        var pods = await this.GetExecutorPods(job, task);

        // container statuses
        var containers = pods.FirstOrDefault()?.Status?.ContainerStatuses ?? Enumerable.Empty<V1ContainerStatus>();

        // try getting the main container
        var mainContainer = containers.FirstOrDefault(container => container.Name == GetMainContainerName(task));

        // the start time
        var startTime = default(DateTime?);
        
        // if container is in running state and there is a valid start time (after task was created)
        if (mainContainer?.State?.Running is { StartedAt: not null } && mainContainer.State.Running.StartedAt >= task.Created)
        {
            startTime = mainContainer.State.Running.StartedAt;
        }
        
        // if container is in terminated state and there is a valid start time (after task was created)
        if (mainContainer?.State?.Terminated is { StartedAt: not null } && mainContainer.State.Terminated.StartedAt >= task.Created)
        {
            startTime = mainContainer.State.Terminated.StartedAt;
        }
        
        return new TaskK8sStatusResult
        {
            ObjectState = K8sObjectState.OK,
            StartTime = startTime,
            CompletionTime = mainContainer?.State?.Terminated?.FinishedAt ?? batchJob.Status?.CompletionTime,
            Succeeded = mainContainer?.State?.Terminated?.ExitCode == 0
        };
    }

    /// <summary>
    /// Gets the task logs
    /// </summary>
    /// <param name="job">The job</param>
    /// <param name="task">The task</param>
    /// <returns></returns>
    public override async Task<Stream> GetTaskLogs(JobModel job, JobTaskModel task)
    {
        // get executor pods
        var pods = await this.GetExecutorPods(job, task);

        // no executor pod
        if (pods.Count == 0)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_KUBERNETES_STATE, "No executor pod found").AsException();
        }

        // take the first executor pod (should be one anyway)
        var pod = pods.First();
        
        // return the logs
        return await this.GetPodLogs(pod);
    }

    /// <summary>
    /// Gets the task instance name
    /// </summary>
    /// <param name="task">The task</param>
    /// <returns></returns>
    private static string GetInstanceName(JobTaskModel task)
    {
        return $"shoc-task-{task.Sequence}";
    }
    
    /// <summary>
    /// Gets the task instance name
    /// </summary>
    /// <param name="task">The task</param>
    /// <returns></returns>
    private static string GetMainContainerName(JobTaskModel task)
    {
        return $"{GetInstanceName(task)}-container";
    }
}