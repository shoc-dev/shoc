using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using k8s.Models;
using Microsoft.AspNetCore.DataProtection;
using Shoc.ApiCore.GrpcClient;
using Shoc.Cluster.Data;
using Shoc.Cluster.Model.Cluster;
using Shoc.Core.Kubernetes;

namespace Shoc.Cluster.Services;

/// <summary>
/// The cluster instance service
/// </summary>
public class ClusterInstanceService : ClusterServiceBase
{
    /// <summary>
    /// The kubernetes service
    /// </summary>
    private readonly K8sService k8SService;
    
    /// <summary>
    /// The resource parser 
    /// </summary>
    private readonly ResourceParser resourceParser;

    /// <summary>
    /// Creates new instance of registry service
    /// </summary>
    /// <param name="registryRepository">The registry repository</param>
    /// <param name="configurationProtectionProvider">The configuration protection provider</param>
    /// <param name="grpcClientProvider">The grpc client provider</param>
    /// <param name="k8sService">The k8s service</param>
    /// <param name="resourceParser">The resource parser</param>
    public ClusterInstanceService(IClusterRepository registryRepository, ConfigurationProtectionProvider configurationProtectionProvider, IGrpcClientProvider grpcClientProvider, K8sService k8sService, ResourceParser resourceParser) : 
        base(registryRepository, configurationProtectionProvider, grpcClientProvider)
    {
        this.k8SService = k8sService;
        this.resourceParser = resourceParser;
    }

    /// <summary>
    /// Perform a dry test of configuration for a new object
    /// </summary>
    /// <returns></returns>
    public async Task<ClusterConnectionTestedModel> Ping(ClusterConnectionTestModel input)
    {
        // perform the test and return the result
        var result = await this.k8SService.GetNodes(new KubeContext{ Config = input.Configuration});
        
        // return existing object
        return new ClusterConnectionTestedModel
        {
            NodesCount = result.Count
        };
    }
    
    /// <summary>
    /// Gets the connectivity info by id
    /// </summary>
    /// <param name="workspaceId"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<ClusterConnectivityModel> GetConnectivityById(string workspaceId, string id)
    {
        // ensure object exists
        var result = await this.RequireClusterById(workspaceId, id);

        // the protector instance
        var protector = this.configurationProtectionProvider.Create(workspaceId);

        // the kube config
        var config = protector.Unprotect(result.Configuration);
        
        // indicate if cluster config is given (by default, consider configured if non-empty config is given)
        var configured = !string.IsNullOrWhiteSpace(config);
        
        // the kube context
        var context = default(KubeContext);
        
        try
        {
            // in case of non-empty config try to validate the schema
            if (configured)
            {
                // try building the kube context from given configuration
                context = new KubeContext { Config = config };
            }
        }
        catch (Exception)
        {
            configured = false;
        }

        // not configured and no need to try to connect
        if (!configured)
        {
            return new ClusterConnectivityModel
            {
                Id = id,
                WorkspaceId = workspaceId,
                Configured = false,
                Connected = false,
                Message = $"The cluster {result.Name} configuration is empty or malformed",
                NodesCount = null,
                Updated = DateTime.Now
            };
        }
        
        // indicate if cluster is not connected by default
        bool connected;
        
        // connectivity message
        string message;

        // the nodes count
        long? nodesCount = null;
        
        try
        {
            // do a basic operation on the cluster
            nodesCount = (await this.k8SService.GetNodes(context)).Count;

            // successful connection message
            message = $"Successfully connected to cluster {result.Name}";

            // mark as connected
            connected = true;

        }
        catch (Exception e)
        {
            // not connected
            connected = false;
            
            // error message
            message = $"Could not connect to cluster {result.Name} due to an error: {e.Message}";
        }

        // return the result
        return new ClusterConnectivityModel
        {
            Id = result.Id,
            WorkspaceId = result.WorkspaceId,
            Connected = connected,
            Configured = true,
            Message = message,
            NodesCount = nodesCount,
            Updated = DateTime.Now
        };
    }
    
    /// <summary>
    /// Gets the connectivity info by id
    /// </summary>
    /// <param name="workspaceId"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<IEnumerable<ClusterNodeModel>> GetNodesById(string workspaceId, string id)
    {
        // ensure object exists
        var cluster = await this.RequireClusterById(workspaceId, id);

        // the protector instance
        var protector = this.configurationProtectionProvider.Create(workspaceId);

        // the kube context
        var context = new KubeContext { Config = protector.Unprotect(cluster.Configuration) };

        // load nodes via API
        var nodes = await this.k8SService.GetNodes(context);

        // node metrics
        IDictionary<string, NodeMetrics> nodeMetrics;

        try
        {
            // try load node metrics from metrics-server (will work if installed)
            nodeMetrics = await this.k8SService.GetNodeMetrics(context);
        }
        catch (Exception)
        {
            // if no luck, we will not have current usage data
            nodeMetrics = new Dictionary<string, NodeMetrics>();
        }
        
        // cluster nodes
        var result = new List<ClusterNodeModel>();
        
        // process all nodes 
        foreach (var node in nodes)
        {
            // get allocatable resources
            var allocatableCpu = node.Status.Allocatable.TryGetValue(WellKnownResources.CPU, out var aCpu) ? aCpu : null;
            var allocatableMemory = node.Status.Allocatable.TryGetValue(WellKnownResources.MEMORY, out var aMemory) ? aMemory : null;
            var allocatableNvidiaGpu = node.Status.Allocatable.TryGetValue(WellKnownResources.NVIDIA_GPU, out var aNvidiaGpu) ? aNvidiaGpu : null;
            var allocatableAmdGpu = node.Status.Allocatable.TryGetValue(WellKnownResources.AMD_GPU, out var aAmdGpu) ? aAmdGpu : null;

            // get capacity resources
            var capacityCpu = node.Status.Capacity.TryGetValue(WellKnownResources.CPU, out var cCpu) ? cCpu : null;
            var capacityMemory = node.Status.Capacity.TryGetValue(WellKnownResources.MEMORY, out var cMemory) ? cMemory : null;
            var capacityNvidiaGpu = node.Status.Capacity.TryGetValue(WellKnownResources.NVIDIA_GPU, out var cNvidiaGpu) ? cNvidiaGpu : null;
            var capacityAmdGpu = node.Status.Capacity.TryGetValue(WellKnownResources.AMD_GPU, out var cAmdGpu) ? cAmdGpu : null;

            // try getting node metrics for the given node
            var metrics = nodeMetrics.TryGetValue(node.Name(), out var nodeMetric) ? nodeMetric : null;

            // get usage data if available or empty
            var usage = metrics?.Usage ?? new Dictionary<string, ResourceQuantity>();
            
            // get usage resources
            var usageCpu = usage.TryGetValue(WellKnownResources.CPU, out var uCpu) ? uCpu : null;
            var usageMemory = usage.TryGetValue(WellKnownResources.MEMORY, out var uMemory) ? uMemory : null;
            
            // build cluster node
            var clusterNode = new ClusterNodeModel
            {
                Name = node.Name(),
                Labels = node.Metadata.Labels ?? new Dictionary<string, string>(),
                AllocatableCpu = this.resourceParser.ParseToMillicores(allocatableCpu?.ToString() ?? "0") ?? 0,
                AllocatableMemory = this.resourceParser.ParseToBytes(allocatableMemory?.ToString() ?? "0") ?? 0,
                AllocatableNvidiaGpu = this.resourceParser.ParseToGpu(allocatableNvidiaGpu?.ToString() ?? "0") ?? 0,
                AllocatableAmdGpu = this.resourceParser.ParseToGpu(allocatableAmdGpu?.ToString() ?? "0") ?? 0,
                CapacityCpu = this.resourceParser.ParseToMillicores(capacityCpu?.ToString() ?? "0") ?? 0,
                CapacityMemory = this.resourceParser.ParseToBytes(capacityMemory?.ToString() ?? "0") ?? 0,
                CapacityNvidiaGpu = this.resourceParser.ParseToGpu(capacityNvidiaGpu?.ToString() ?? "0") ?? 0,
                CapacityAmdGpu = this.resourceParser.ParseToGpu(capacityAmdGpu?.ToString() ?? "0") ?? 0,
                UsedCpu = this.resourceParser.ParseToMillicores(usageCpu?.ToString()),
                UsedMemory = this.resourceParser.ParseToBytes(usageMemory?.ToString())
            };
            
            // add to result
            result.Add(clusterNode);
        }

        return result;
    }
}