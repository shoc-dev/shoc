namespace Shoc.Job.K8s.Model;

/// <summary>
/// The descriptor of node resources
/// </summary>
public class NodeResourceResult
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
    /// The CPU resources available on the node
    /// </summary>
    public string Cpu { get; set; }
    
    /// <summary>
    /// The Memory resources available on the node
    /// </summary>
    public string Memory { get; set; }
    
    /// <summary>
    /// The Nvidia GPU resources available on the node
    /// </summary>
    public string NvidiaGpu { get; set; }
    
    /// <summary>
    /// The AMD GPU resources available on the node
    /// </summary>
    public string AmdGpu { get; set; }
}