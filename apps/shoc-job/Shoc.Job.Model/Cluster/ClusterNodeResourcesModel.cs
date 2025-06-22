namespace Shoc.Job.Model.Cluster;

/// <summary>
/// The cluster node resource model
/// </summary>
public class ClusterNodeResourcesModel
{
    /// <summary>
    /// The name of the node
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Indicate if jobs can be scheduled on the node
    /// </summary>
    public bool CanSchedule { get; set; }
    
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
    public long? NvidiaGpu { get; set; }
    
    /// <summary>
    /// The amount of AMD GPU units to allocate
    /// </summary>
    public long? AmdGpu { get; set; }
    
    /// <summary>
    /// Memory (bytes) per CPU (millicores) ratio
    /// </summary>
    public decimal? MemoryCpuRatio { get; set; }
    
    /// <summary>
    /// Memory (bytes) per Nvidia GPU ratio
    /// </summary>
    public decimal? MemoryNvidiaGpuRatio { get; set; }
    
    /// <summary>
    /// CPU (millicores) per Nvidia GPU ratio
    /// </summary>
    public decimal? CpuNvidiaGpuRatio { get; set; }
    
    /// <summary>
    /// Memory (bytes) per AMD GPU ratio
    /// </summary>
    public decimal? MemoryAmdGpuRatio { get; set; }
    
    /// <summary>
    /// CPU (bytes) per AMD GPU ratio
    /// </summary>
    public decimal? CpuAmdGpuRatio { get; set; }
}