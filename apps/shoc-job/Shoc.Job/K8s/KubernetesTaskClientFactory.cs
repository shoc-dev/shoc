using Shoc.Core;
using Shoc.Job.K8s.TaskClients;
using Shoc.Job.Model;
using Shoc.Job.Model.JobTask;

namespace Shoc.Job.K8s;

/// <summary>
/// The Kubernetes task client factory 
/// </summary>
public class KubernetesTaskClientFactory
{
    /// <summary>
    /// Creates a Kubernetes task client based on the runtime type
    /// </summary>
    /// <param name="config">The cluster configuration</param>
    /// <param name="type">The runtime type</param>
    /// <returns></returns>
    public IKubernetesTaskClient Create(string config, string type)
    {
        return type switch
        {
            JobTaskTypes.FUNCTION => new FunctionKubernetesTaskClient(config),
            JobTaskTypes.MPI => new MpiKubernetesTaskClient(config),
            _ => throw ErrorDefinition.Validation(JobErrors.INVALID_RUNTIME_TYPE, $"The type '{type}' is not supported")
                .AsException()
        };
    }
}