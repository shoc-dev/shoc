using System.Collections.Generic;

namespace Shoc.Job.Model.Job;

/// <summary>
/// The MPI resource specifications proportions
/// </summary>
public class JobRunManifestSpecMpiResourceProportions
{
    /// <summary>
    /// The total cpu budget
    /// </summary>
    public long? TotalCpu { get; set; }
    
    /// <summary>
    /// The total memory budget
    /// </summary>
    public long? TotalMemory { get; set; }
    
    /// <summary>
    /// The total nvidia.com/gpu budget
    /// </summary>
    public long? TotalNvidiaGpu { get; set; }
    
    /// <summary>
    /// The total amd.com/gpu budget
    /// </summary>
    public long? TotalAmdGpu { get; set; }
    
    /// <summary>
    /// The launcher cpu budget
    /// </summary>
    public long? LauncherCpu { get; set; }
    
    /// <summary>
    /// The launcher memory budget
    /// </summary>
    public long? LauncherMemory { get; set; }
    
    /// <summary>
    /// The launcher nvidia.com/gpu budget
    /// </summary>
    public long? LauncherNvidiaGpu { get; set; }
    
    /// <summary>
    /// The launcher amd.com/gpu budget
    /// </summary>
    public long? LauncherAmdGpu { get; set; }
    
    /// <summary>
    /// The total (launcher subtracted) cpu budget
    /// </summary>
    public long? TotalSubtractedCpu { get; set; }
    
    /// <summary>
    /// The total (launcher subtracted) memory budget
    /// </summary>
    public long? TotalSubtractedMemory { get; set; }
    
    /// <summary>
    /// The total (launcher subtracted) nvidia.com/gpu budget
    /// </summary>
    public long? TotalSubtractedNvidiaGpu { get; set; }
    
    /// <summary>
    /// The total (launcher subtracted) amd.com/gpu budget
    /// </summary>
    public long? TotalSubtractedAmdGpu { get; set; }
    
    /// <summary>
    /// The worker cpu budget
    /// </summary>
    public long? WorkerCpu { get; set; }
    
    /// <summary>
    /// The worker memory budget
    /// </summary>
    public long? WorkerMemory { get; set; }
    
    /// <summary>
    /// The worker nvidia.com/gpu budget
    /// </summary>
    public long? WorkerNvidiaGpu { get; set; }
    
    /// <summary>
    /// The worker amd.com/gpu budget
    /// </summary>
    public long? WorkerAmdGpu { get; set; }
    
    /// <summary>
    /// The cpu proportion ((total - launcher?) / worker)
    /// </summary>
    public decimal? CpuProportion { get; set; }
    
    /// <summary>
    /// The memory proportion ((total - launcher?) / worker)
    /// </summary>
    public decimal? MemoryProportion { get; set; }
    
    /// <summary>
    /// The nvidia.com/gpu proportion ((total - launcher?) / worker)
    /// </summary>
    public decimal? NvidiaGpuProportion { get; set; }
    
    /// <summary>
    /// The amd.com/gpu proportion ((total - launcher?) / worker)
    /// </summary>
    public decimal? AmdGpuProportion { get; set; }
    
    /// <summary>
    /// The set of definite proportions (resources are known) 
    /// </summary>
    public Dictionary<string, decimal> DefiniteProportions { get; set; }
}