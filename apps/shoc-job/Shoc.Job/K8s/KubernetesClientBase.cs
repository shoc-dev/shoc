using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using Shoc.Core;
using Shoc.Core.Kubernetes;
using Shoc.Core.Kubernetes.Model.Mpi.V2Beta1;
using Shoc.Job.K8s.Model;
using Shoc.Job.Model;
using Shoc.Job.Model.Job;
using Shoc.Job.Model.JobTask;

namespace Shoc.Job.K8s;

/// <summary>
/// The base client for Kubernetes
/// </summary>
public abstract class KubernetesClientBase
{
    /// <summary>
    /// The underlying kubernetes client
    /// </summary>
    protected readonly Kubernetes client;

    /// <summary>
    /// The kubernetes client for job operations
    /// </summary>
    /// <param name="config">The cluster config for authentication</param>
    protected KubernetesClientBase(string config)
    {
        this.client = new Kubernetes(new KubeContext { Config = config }.AsClientConfiguration());
    }

    /// <summary>
    /// Gets the set of default labels
    /// </summary>
    /// <returns></returns>
    protected static IDictionary<string, string> CreateManagedLabels(ManagedMetadata metadata)
    {
        // require name
        if (string.IsNullOrWhiteSpace(metadata.Name))
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_ARGUMENT, "The managed object name is required").AsException();
        }
        
        // require component
        if (string.IsNullOrWhiteSpace(metadata.Component))
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_ARGUMENT, "The managed object component is required").AsException();
        }
        
        // require workspace
        if (string.IsNullOrWhiteSpace(metadata.WorkspaceId))
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_ARGUMENT, "The managed object workspace is required").AsException();
        }
        
        // require job
        if (string.IsNullOrWhiteSpace(metadata.JobId))
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_ARGUMENT, "The managed object job is required").AsException();
        }
        
        var result = new Dictionary<string, string>
        {
            [WellKnownLabels.NAME] = metadata.Name ?? string.Empty,
            [WellKnownLabels.INSTANCE] = metadata.Instance ?? string.Empty,
            [WellKnownLabels.VERSION] = K8sConstants.SHOC_DEFAULT_OBJECT_VERSION,
            [WellKnownLabels.COMPONENT] = metadata.Component,
            [WellKnownLabels.PART_OF] = metadata.PartOf ?? metadata.Component,
            [WellKnownLabels.MANAGED_BY] = K8sConstants.SHOC_MANAGED_BY,
            [ShocK8sLabels.SHOC_WORKSPACE] = metadata.WorkspaceId,
            [ShocK8sLabels.SHOC_JOB] = metadata.JobId,
            [ShocK8sLabels.SHOC_JOB_TASK] = metadata.TaskId ?? string.Empty,
        };

        return result;
    }
    
    /// <summary>
    /// Gets the kubernetes job based on the selector
    /// </summary>
    /// <param name="job">The job instance</param>
    /// <param name="task">The task instance</param>
    /// <returns></returns>
    protected async Task<IList<V1Job>> GetKubernetesJobs(JobModel job, JobTaskModel task)
    {
        // load matching jobs
        var matches = await this.client.BatchV1.ListNamespacedJobAsync(
            job.Namespace,
            labelSelector: $"{ShocK8sLabels.SHOC_JOB_TASK}={task.Id}"
        );

        // return matching items
        return matches.Items;
    }
    
    /// <summary>
    /// Gets the kubernetes job based on the selector
    /// </summary>
    /// <param name="job">The job instance</param>
    /// <param name="task">The task instance</param>
    /// <returns></returns>
    protected async Task<IList<MPIJob>> GetKubernetesMpiJobs(JobModel job, JobTaskModel task)
    {
        // load matching jobs
        var response = await this.client.ListNamespacedCustomObjectAsync<MPIJobList>(
            group: "kubeflow.org",
            version: "v2beta1",
            namespaceParameter: job.Namespace,
            plural: "mpijobs",
            labelSelector: $"{ShocK8sLabels.SHOC_JOB_TASK}={task.Id}"
        );
        
        // return matching items
        return response.Items;
    }
    
    /// <summary>
    /// Gets the stream of logs based on the pod
    /// </summary>
    /// <param name="pod">The pod to watch</param>
    /// <returns></returns>
    protected async Task<Stream> GetPodLogs(V1Pod pod)
    {
        // get the logs stream
        var result = await this.client.CoreV1.ReadNamespacedPodLogAsync(pod.Name(), pod.Namespace(), follow: true);

        // return matching items
        return result;
    }
    
    /// <summary>
    /// Gets the kubernetes job based on the selector
    /// </summary>
    /// <param name="job">The job instance</param>
    /// <param name="task">The task instance</param>
    /// <returns></returns>
    protected async Task<IList<V1Pod>> GetExecutorPods(JobModel job, JobTaskModel task)
    {
        // load matching jobs
        var matches = await this.client.ListNamespacedPodAsync(
            job.Namespace,
            labelSelector: $"{ShocK8sLabels.SHOC_JOB_TASK}={task.Id},{ShocK8sLabels.SHOC_POD_ROLE}={ShocK8sPodRoles.EXECUTOR}"
        );

        // return matching items
        return matches.Items;
    }
    
    /// <summary>
    /// Gets security context from the runtime configuration
    /// </summary>
    /// <param name="runtime">The runtime</param>
    /// <returns></returns>
    protected static V1SecurityContext GetSecurityContext(JobTaskRuntimeModel runtime)
    {
        return new V1SecurityContext
        {
            RunAsNonRoot = runtime.Uid.HasValue,
            RunAsUser = runtime.Uid
        };
    }
    
    /// <summary>
    /// Gets pod security context from the runtime configuration
    /// </summary>
    /// <param name="runtime">The runtime</param>
    /// <returns></returns>
    protected static V1PodSecurityContext GetPodSecurityContext(JobTaskRuntimeModel runtime)
    {
        return new V1PodSecurityContext
        {
            RunAsNonRoot = runtime.Uid.HasValue,
            RunAsUser = runtime.Uid
        };
    }
    
    /// <summary>
    /// Get indexer environment variables
    /// </summary>
    /// <param name="task">The task model</param>
    /// <returns></returns>
    protected static List<V1EnvVar> GetIndexerVars(JobTaskModel task)
    {
        return
        [
            new V1EnvVar
            {
                Name = task.ArrayCounter,
                Value = task.ArrayReplicas.ToString()
            },

            new V1EnvVar
            {
                Name = task.ArrayIndexer,
                Value = task.Sequence.ToString()
            }
        ];
    }
    
    /// <summary>
    /// Gets the env sources from the input
    /// </summary>
    /// <param name="input">The task input</param>
    /// <returns></returns>
    protected static IList<V1EnvFromSource> GetEnvSources(InitTaskInput input)
    {
        // the result collection
        var result = new List<V1EnvFromSource>();

        // add secrets
        result.AddRange(input.SharedEnv.Secrets.Select(sec => new V1EnvFromSource
        {
            SecretRef = new V1SecretEnvSource
            {
                Name = sec.Name()
            }
        }));
        
        // add config maps
        result.AddRange(input.SharedEnv.ConfigMaps.Select(sec => new V1EnvFromSource
        {
            ConfigMapRef = new V1ConfigMapEnvSource
            {
                Name = sec.Name()
            }
        }));
        
        // return result
        return result;
    }
    
    /// <summary>
    /// Gets the list of pull secrets based on the task input
    /// </summary>
    /// <param name="input">The input</param>
    /// <returns></returns>
    protected static IList<V1LocalObjectReference> GetPullSecrets(InitTaskInput input)
    {
        return new List<V1LocalObjectReference>
        {
            new()
            {
                Name = input.PullSecret.PullSecret.Name()
            }
        };
    }

    /// <summary>
    /// Gets the task resource requirements dictionary
    /// </summary>
    /// <param name="task">The task instance</param>
    /// <returns></returns>
    protected static Dictionary<string, ResourceQuantity> GetTaskResources(JobTaskModel task)
    {
        // the result map
        var result = new Dictionary<string, ResourceQuantity>();

        // if Memory is requested
        if (task.MemoryRequested.HasValue)
        {
            result[WellKnownResources.MEMORY] = new ResourceQuantity(task.MemoryRequested.ToString());
        }
        
        // if CPU is requested
        if (task.CpuRequested.HasValue)
        {
            result[WellKnownResources.CPU] = new ResourceQuantity($"{task.CpuRequested}m");
        }
        
        // if Nvidia GPU is requested
        if (task.NvidiaGpuRequested.HasValue)
        {
            result[WellKnownResources.NVIDIA_GPU] = new ResourceQuantity(task.NvidiaGpuRequested.ToString());
        }
        
        // if AMD GPU is requested
        if (task.AmdGpuRequested.HasValue)
        {
            result[WellKnownResources.AMD_GPU] = new ResourceQuantity(task.AmdGpuRequested.ToString());
        }
        
        // return result
        return result;
    }
}