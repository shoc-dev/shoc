using System.Collections.Generic;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using Shoc.Core;
using Shoc.Job.K8s.Model;
using Shoc.Job.Model;
using Shoc.Job.Model.Job;

namespace Shoc.Job.K8s.JobClients;

/// <summary>
/// The kubernetes MPI job client
/// </summary>
public class MpiKubernetesJobClient : BaseKubernetesJobClient
{
    /// <summary>
    /// The kubernetes client for job operations
    /// </summary>
    /// <param name="config">The cluster config for authentication</param>
    public MpiKubernetesJobClient(string config) : base(config)
    {
    }

    /// <summary>
    /// Initializes the shared runtime
    /// </summary>
    /// <param name="job">The job object</param>
    /// <returns></returns>
    public override async Task<InitSharedRuntimeResult> InitSharedRuntime(JobModel job)
    {
        // load the contents of the entrypoint file
        var entrypoint = await LoadFileOrNull(GetScriptsPath("K8s", "Scripts", "Mpi", "entrypoint.sh"));

        // make sure entrypoint exists
        if (string.IsNullOrWhiteSpace(entrypoint))
        {
            throw ErrorDefinition.Validation(JobErrors.UNKNOWN_ERROR, "The MPI runtime entrypoint script could not be found").AsException();
        }
        
        // create default labels 
        var labels = CreateManagedLabels(new ManagedMetadata
        {
            Name = K8sConstants.SHOC_SHARED_RUNTIME_CONFIG,
            Component = ShocK8sComponents.ENVIRONMENT,
            PartOf = ShocK8sComponents.JOB,
            WorkspaceId = job.WorkspaceId,
            JobId = job.Id
        });
        
        // the config instance
        var config = new V1ConfigMap
        {
            Metadata = new V1ObjectMeta
            {
                Name = K8sConstants.SHOC_SHARED_RUNTIME_CONFIG,
                NamespaceProperty = job.Namespace,
                Labels = labels
            },
            Data = new Dictionary<string, string>
            {
                { K8sConstants.SHOC_SHARED_RUNTIME_CONFIG_ENTRYPOINT_KEY, entrypoint }
            }
        };

        // create in the cluster
        var created = await this.client.CreateNamespacedConfigMapAsync(config, job.Namespace);

        // build and return the result
        return new InitSharedRuntimeResult
        {
            RuntimeConfig = created
        };
    }
}