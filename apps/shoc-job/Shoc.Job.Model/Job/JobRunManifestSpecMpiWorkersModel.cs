namespace Shoc.Job.Model.Job;

/// <summary>
/// The job run manifest specification of MPI workers model
/// </summary>
public class JobRunManifestSpecMpiWorkersModel
{
    /// <summary>
    /// The number of replicas
    /// </summary>
    public long? Replicas { get; set; }
    
    /// <summary>
    /// The distribution unit (type of resource: cpu, memory, etc.)
    /// </summary>
    public string DistributionUnit { get; set; }
    
    /// <summary>
    /// The distribution strategy
    /// </summary>
    public string DistributionStrategy { get; set; }
    
    /// <summary>
    /// The per-worker resources
    /// </summary>
    public JobRunManifestResourcesModel Resources { get; set; }
}