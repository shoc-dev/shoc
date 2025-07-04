namespace Shoc.Job.K8s.JobClients;

/// <summary>
/// The kubernetes job function client
/// </summary>
public class FunctionKubernetesJobClient : BaseKubernetesJobClient
{
    /// <summary>
    /// The kubernetes client for job operations
    /// </summary>
    /// <param name="config">The cluster config for authentication</param>
    public FunctionKubernetesJobClient(string config) : base(config)
    {
    }
}