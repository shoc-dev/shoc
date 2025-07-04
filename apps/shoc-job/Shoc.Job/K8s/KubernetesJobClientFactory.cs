using Shoc.Core;
using Shoc.Job.K8s.JobClients;
using Shoc.Job.Model;
using Shoc.Job.Model.JobTask;

namespace Shoc.Job.K8s;

/// <summary>
/// The Kubernetes job client factory 
/// </summary>
public class KubernetesJobClientFactory
{
    /// <summary>
    /// Creates a Kubernetes task client based on the runtime type
    /// </summary>
    /// <param name="config">The cluster configuration</param>
    /// <param name="type">The runtime type</param>
    /// <returns></returns>
    public IKubernetesJobClient Create(string config, string type)
    {
        return type switch
        {
            JobTaskTypes.FUNCTION => new FunctionKubernetesJobClient(config),
            JobTaskTypes.MPI => new MpiKubernetesJobClient(config),
            _ => throw ErrorDefinition.Validation(JobErrors.INVALID_RUNTIME_TYPE, $"The type '{type}' is not supported")
                .AsException()
        };
    }
}