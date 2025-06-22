namespace Shoc.Job.Model.Cluster;

/// <summary>
/// The cluster total resources model
/// </summary>
public class ClusterTotalResourcesModel
{
    /// <summary>
    /// The number of CPU units to allocate
    /// </summary>
    public long Cpu { get; set; }
    
    /// <summary>
    /// The amount of memory to allocate
    /// </summary>
    public long Memory { get; set; }
    
    /// <summary>
    /// The amount of NVIDIA GPU units to allocate
    /// </summary>
    public long NvidiaGpu { get; set; }
    
    /// <summary>
    /// The amount of AMD GPU units to allocate
    /// </summary>
    public long AmdGpu { get; set; }
}