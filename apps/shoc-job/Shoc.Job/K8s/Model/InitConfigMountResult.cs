using k8s.Models;

namespace Shoc.Job.K8s.Model;

/// <summary>
/// The configuration mount result
/// </summary>
public class InitConfigMountResult
{
    /// <summary>
    /// The config map
    /// </summary>
    public V1ConfigMap ConfigMap { get; set; }
    
    /// <summary>
    /// The key to mount
    /// </summary>
    public string ConfigKey { get; set; }
}