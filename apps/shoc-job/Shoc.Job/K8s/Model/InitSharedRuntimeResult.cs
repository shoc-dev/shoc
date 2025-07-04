using System.Collections.Generic;
using k8s.Models;

namespace Shoc.Job.K8s.Model;

/// <summary>
/// The Kubernetes config for runtime support
/// </summary>
public class InitSharedRuntimeResult
{
    /// <summary>
    /// The config mounts
    /// </summary>
    public V1ConfigMap RuntimeConfig { get; set; }
}