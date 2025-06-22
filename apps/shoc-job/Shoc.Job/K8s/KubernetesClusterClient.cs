using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using k8s;
using Shoc.Core.Kubernetes;
using Shoc.Job.K8s.Model;

namespace Shoc.Job.K8s;

/// <summary>
/// The kubernetes cluster client
/// </summary>
public class KubernetesClusterClient : KubernetesClientBase, IDisposable
{
    /// <summary>
    /// The kubernetes client for job operations
    /// </summary>
    /// <param name="config">The cluster config for authentication</param>
    public KubernetesClusterClient(string config) : base(config)
    {
    }
    
    /// <summary>
    /// Gets the node resources
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<NodeResourceResult>> GetNodeResources()
    {
        // list nodes
        var nodes = await client.ListNodeAsync();

        // final results
        var result = new List<NodeResourceResult>();

        // process each node
        foreach (var node in nodes)
        {
            // get the condition of type Ready if any
            var readyCondition = node.Status.Conditions.FirstOrDefault(c => c.Type == "Ready");
            
            // ready only if condition is there and the status is true
            var isReady = readyCondition is { Status: "True" };
            
            // the scheduling is allowed if no taints or no taints that disallowing the scheduling
            var allowedScheduling = node.Spec.Taints == null || !node.Spec.Taints.Any(taint =>  taint.Effect is "NoSchedule" or "NoExecute");
            
            // can schedule if "Unschedulable" field is false
            var canSchedule = !node.Spec.Unschedulable.GetValueOrDefault(false);
            
            result.Add(new NodeResourceResult
            {
                Name = node.Metadata.Name,
                CanSchedule = isReady && allowedScheduling && canSchedule,
                Cpu = node.Status.Allocatable.TryGetValue(WellKnownResources.CPU, out var cpuValue) ? cpuValue?.ToString() ?? "0" : "0",
                Memory = node.Status.Allocatable.TryGetValue(WellKnownResources.MEMORY, out var memoryValue) ? memoryValue?.ToString() ?? "0" : "0",
                NvidiaGpu = node.Status.Allocatable.TryGetValue(WellKnownResources.NVIDIA_GPU, out var nvidiaValue) ? nvidiaValue?.ToString() : null,
                AmdGpu = node.Status.Allocatable.TryGetValue(WellKnownResources.AMD_GPU, out var amdValue) ? amdValue?.ToString() : null
            });
        }

        // the result collection
        return result;
    }
    
    /// <summary>
    /// Disposes the client
    /// </summary>
    public void Dispose()
    {
        this.client?.Dispose();
        GC.SuppressFinalize(this);
    }
}