using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using Shoc.Core;
using Shoc.Core.Kubernetes.Model.Mpi.V2Beta1;
using Shoc.Job.K8s.Model;
using Shoc.Job.Model;
using Shoc.Job.Model.Job;
using Shoc.Job.Model.JobTask;

namespace Shoc.Job.K8s.TaskClients;

/// <summary>
/// The MPI Kubernetes task client
/// </summary>
public class MpiKubernetesTaskClient : BaseKubernetesTaskClient
{
    /// <summary>
    /// The custom resource definition of MPI Job (from Kubeflow)
    /// </summary>
    private const string MPI_JOB_CRD_NAME = "mpijobs.kubeflow.org";
    
    /// <summary>
    /// Creates new instance of MPI task client
    /// </summary>
    /// <param name="config">The cluster configuration</param>
    public MpiKubernetesTaskClient(string config) : base(config, JobTaskTypes.FUNCTION)
    {
    }

    /// <summary>
    /// Checks if cluster supports the given task type
    /// </summary>
    /// <returns></returns>
    public override async Task<bool> IsSupported()
    {   
        // get the list of CRDs
        var crds = await client.ListCustomResourceDefinitionAsync();
        
        // check if our CRD exists in the list
        return crds.Items.Any(crd => crd.Metadata.Name.Equals(MPI_JOB_CRD_NAME, StringComparison.OrdinalIgnoreCase));
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
        
        // get the implementation
        var implementation = MapMpiImplementation(input.Runtime.Type);
        
        // indicate if is openmpi
        var isOpenMpi = implementation == "OpenMPI";
        
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
        
        var instance = new MPIJob
        {
            ApiVersion = "kubeflow.org/v2beta1",
            Kind = "MPIJob",
            Metadata = new V1ObjectMeta
            {
                Name = instanceName,
                NamespaceProperty = input.Namespace,
                Labels = labels
            },
            Spec = new MPIJobSpec
            {
                SlotsPerWorker = 1,
                RunLauncherAsWorker = false,
                SSHAuthMountPath = $"/home/{input.Runtime.User}/.ssh",
                LauncherCreationPolicy = "WaitForWorkersReady",
                MPIImplementation = MapMpiImplementation(input.Runtime.Implementation),
                RunPolicy = new RunPolicy
                {
                    CleanPodPolicy = "Running",
                    TtlSecondsAfterFinished = (int)TimeSpan.FromDays(30).TotalSeconds,
                    BackoffLimit = 0,
                    ActiveDeadlineSeconds = (int)TimeSpan.FromDays(30).TotalSeconds,
                    Suspend = false,
                },
                MPIReplicaSpecs = new Dictionary<string, ReplicaSpec>
                {
                    {"Launcher", new ReplicaSpec
                        {
                            Replicas = 1,
                            RestartPolicy = DEFAULT_RESTART_POLICY,
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
                                    ImagePullSecrets = GetPullSecrets(input),
                                    RestartPolicy = DEFAULT_RESTART_POLICY,
                                    ServiceAccountName = input.ServiceAccount,
                                    SecurityContext = GetPodSecurityContext(input.Runtime),
                                    Containers = new List<V1Container>
                                    {
                                        new()
                                        {
                                            EnvFrom = GetEnvSources(input),
                                            Name = GetLauncherContainerName(input.Task),
                                            Image = input.PullSecret.Image,
                                            Env = GetIndexerVars(input.Task),
                                            SecurityContext = GetSecurityContext(input.Runtime),
                                            Command = isOpenMpi ? ["mpirun"] : null,
                                            Args = (isOpenMpi ? ["-np", "4"] : new[] {"mpirun", "-np", "4"}).Concat(input.Runtime.Entrypoint).ToList()
                                        }
                                    }
                                }
                            }
                        }
                    },
                    {"Worker", new ReplicaSpec
                    {
                        Replicas = 1,
                        RestartPolicy = DEFAULT_RESTART_POLICY,
                        Template = new V1PodTemplateSpec
                        {
                            Metadata = new V1ObjectMeta
                            {
                                Labels = new Dictionary<string, string>(labels)
                                {
                                    [ShocK8sLabels.SHOC_POD_ROLE] = ShocK8sPodRoles.WORKER
                                }
                            },
                            Spec = new V1PodSpec
                            {
                                ImagePullSecrets = GetPullSecrets(input),
                                RestartPolicy = DEFAULT_RESTART_POLICY,
                                ServiceAccountName = input.ServiceAccount,
                                SecurityContext = GetPodSecurityContext(input.Runtime),
                                Containers = new List<V1Container>
                                {
                                    new()
                                    {
                                        EnvFrom = GetEnvSources(input),
                                        Name = GetWorkerContainerName(input.Task),
                                        Image = input.PullSecret.Image,
                                        Env = GetIndexerVars(input.Task),
                                        SecurityContext = GetSecurityContext(input.Runtime),
                                        Command = ["/usr/sbin/sshd"],
                                        Args = ["-De", "-f", $"/home/{input.Runtime.User}/.sshd_config"],
                                        ReadinessProbe = isOpenMpi ? null : new V1Probe
                                        {
                                            InitialDelaySeconds = 2,
                                            TcpSocket = new V1TCPSocketAction
                                            {
                                                Port = 2222
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    }
                }

            }
        };

        // submit the object to the cluster
        await this.client.CreateNamespacedCustomObjectAsync(instance, 
            group: "kubeflow.org", 
            version: "v2beta1",
            namespaceParameter: input.Namespace, 
            plural: "mpijobs");
        
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
        var batchJobs = await this.GetKubernetesMpiJobs(job, task);

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
        var mainContainer = containers.FirstOrDefault(container => container.Name == GetLauncherContainerName(task));

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
    /// Gets the name of the launcher container
    /// </summary>
    /// <param name="task">The task</param>
    /// <returns></returns>
    private static string GetLauncherContainerName(JobTaskModel task)
    {
        return $"{GetInstanceName(task)}-launcher";
    }
    
    /// <summary>
    /// Gets the name of the worker container
    /// </summary>
    /// <param name="task">The task</param>
    /// <returns></returns>
    private static string GetWorkerContainerName(JobTaskModel task)
    {
        return $"{GetInstanceName(task)}-worker";
    }

    /// <summary>
    /// Maps the MPI implementation requested from the runtime
    /// </summary>
    /// <param name="implementation">The given implementation</param>
    /// <returns></returns>
    private static string MapMpiImplementation(string implementation)
    {
        return implementation?.ToLowerInvariant() switch
        {
            "openmpi" => "OpenMPI",
            "mpich" => "MPICH",
            "intel" => "Intel",
            _ => "OpenMPI"
        };
    }
}