using System.Collections.Generic;
using System.Linq;
using Shoc.Core;
using Shoc.Job.Model;
using Shoc.Job.Model.Cluster;
using Shoc.Job.Model.Job;

namespace Shoc.Job.Allocator;

/// <summary>
/// The resource handler base service
/// </summary>
public abstract class JobResourceAllocatorBase
{
    /// <summary>
    /// The single cpu value in millicors
    /// </summary>
    protected const long SINGLE_CPU_VALUE = 1000;
    
    /// <summary>
    /// The maximum amount of memory requested by job (TBs * GBs * MBs * KBs * Bytes) 
    /// </summary>
    protected const long MAX_JOB_REQUESTED_MEMORY = 1L * 1024 * 1024 * 1024 * 1024;

    /// <summary>
    /// The maximum amount of CPU requested by job (N * 1000m)
    /// </summary>
    protected const long MAX_JOB_REQUESTED_CPU = 4096L * 1000;

    /// <summary>
    /// The maximum amount of NVIDIA GPU requested by job (N units)
    /// </summary>
    protected const long MAX_JOB_REQUESTED_NVIDIA_GPU = 4096;
    
    /// <summary>
    /// The maximum amount of AMD GPU requested by job (N units)
    /// </summary>
    protected const long MAX_JOB_REQUESTED_AMD_GPU = 4096;
    
    /// <summary>
    /// The resource formatter
    /// </summary>
    protected readonly JobResourceFormatter jobResourceFormatter;

    /// <summary>
    /// The nodes to allocate against
    /// </summary>
    protected readonly List<ClusterNodeResourcesModel> allNodes;

    /// <summary>
    /// The sub-set of nodes that allow scheduling against
    /// </summary>
    protected readonly List<ClusterNodeResourcesModel> readyNodes;

    /// <summary>
    /// The cluster total resources
    /// </summary>
    protected readonly ClusterTotalResourcesModel clusterTotal;

    /// <summary>
    /// The base service for resource handling 
    /// </summary>
    /// <param name="jobResourceFormatter">The resource formatter</param>
    /// <param name="nodes">The nodes</param>
    protected JobResourceAllocatorBase(JobResourceFormatter jobResourceFormatter, List<ClusterNodeResourcesModel> nodes)
    {
        this.jobResourceFormatter = jobResourceFormatter;
        this.allNodes = nodes;
        this.readyNodes = this.GetReadyNodes();
        this.clusterTotal = this.GetClusterTotal();
    }

    /// <summary>
    /// Returns new manifest instance with enriched resource definitions
    /// </summary>
    /// <param name="input">The input</param>
    /// <returns></returns>
    public abstract JobRunManifestModel Allocate(JobRunManifestModel input);
    
    /// <summary>
    /// Validates the values for the given resources
    /// </summary>
    /// <param name="input">The input to validate</param>
    protected void ValidateResourcesRangeValidity(JobRunManifestResourcesModel input)
    {
        // empty resources are considered valid
        if (input == null)
        {
            return;
        }
        
        // memory should be non-negative
        if (input.Memory <= 0)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_RESOURCES, "Requested memory should be positive").AsException();
        }
        
        // memory should be within limit
        if (input.Memory > MAX_JOB_REQUESTED_MEMORY)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_RESOURCES, "Too much memory requested").AsException();
        }
        
        // CPU should be non-negative
        if (input.Cpu <= 0)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_RESOURCES, "Requested CPU should be positive").AsException();
        }
        
        // CPU should be within limit
        if (input.Cpu > MAX_JOB_REQUESTED_CPU)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_RESOURCES, "Too much CPU requested").AsException();
        }
        
        // Nvidia GPU should be non-negative
        if (input.NvidiaGpu is <= 0)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_RESOURCES, "Requested Nvidia GPU should be positive").AsException();
        }
        
        // Nvidia GPU should be within limit
        if (input.NvidiaGpu is > MAX_JOB_REQUESTED_NVIDIA_GPU)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_RESOURCES, "Too much Nvidia GPU requested").AsException();
        }
        
        // AMD GPU should be non-negative
        if (input.AmdGpu is <= 0)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_RESOURCES, "Requested AMD GPU should be positive").AsException();
        }
        
        // AMD GPU should be within limit
        if (input.AmdGpu is > MAX_JOB_REQUESTED_AMD_GPU)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_RESOURCES, "Too much AMD GPU requested").AsException();
        }
    }
    
    /// <summary>
    /// Validates if the given resources can be scheduled on any of nodes
    /// </summary>
    /// <param name="requirements">The resources to validate against</param>
    protected void ValidateAnyNodeFeasibility(JobRunManifestResourcesModel requirements)
    {
        // count capable nodes
        var capableNodes = this.GetCapableNodes(requirements).Count;

        // if there are no capable nodes to take one task, report not feasible
        if (capableNodes == 0)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_RESOURCES, "None of the nodes has enough allocatable resources").AsException();
        }
    }
    
    /// <summary>
    /// Validates if the resources can be scheduled over the cluster overall
    /// </summary>
    /// <param name="requirements">The resources to validate against</param>
    protected void ValidateClusterFeasibility(JobRunManifestResourcesModel requirements)
    {
        // assuming the cluster is capable
        var capable = true;

        // if CPU is requested make sure cluster has enough 
        if (requirements.Cpu.HasValue)
        {
            capable = requirements.Cpu.Value <= this.clusterTotal.Cpu;
        }
        
        // if memory is requested make sure cluster has enough 
        if (requirements.Memory.HasValue)
        {
            capable = capable && requirements.Memory.Value <= this.clusterTotal.Memory;
        }
        
        // if Nvidia GPU is requested make sure cluster has enough 
        if (requirements.NvidiaGpu.HasValue)
        {
            capable = capable && requirements.NvidiaGpu.Value <= this.clusterTotal.NvidiaGpu;
        }
        
        // if AMD GPU is requested make sure cluster has enough 
        if (requirements.AmdGpu.HasValue)
        {
            capable = capable && requirements.AmdGpu.Value <= this.clusterTotal.AmdGpu;
        }
        
        // if there are no capable nodes to take one task, report not feasible
        if (!capable)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_RESOURCES, "The cluster does not have enough resources for scheduling all the requested MPI resources").AsException();
        }
    }
    
    /// <summary>
    /// Gets the nodes that are ready for scheduling
    /// </summary>
    /// <returns></returns>
    private List<ClusterNodeResourcesModel> GetReadyNodes()
    {
        // only nodes that allow scheduling and have at cpu or memory can be used
        return this.allNodes.Where(node => node.CanSchedule && node.Cpu > 0 && node.Memory > 0 && node.MemoryCpuRatio > 0).ToList();
    }
    
    /// <summary>
    /// Gets all the nodes capable to fit given requirements
    /// </summary>
    /// <param name="requirements">The resources to validate against</param>
    protected List<ClusterNodeResourcesModel> GetCapableNodes(JobRunManifestResourcesModel requirements)
    {
        // capable nodes
        var capableNodes = new List<ClusterNodeResourcesModel>();
        
        // check each node
        foreach (var nodeResource in this.readyNodes)
        {
            // assume node has enough capacity
            var enoughCapacity = true;
            
            // CPU is required
            if (requirements.Cpu.HasValue)
            {
                // still enough if required CPU is less than is available
                enoughCapacity = requirements.Cpu.Value <= nodeResource.Cpu;
            }
            
            // memory is required
            if (requirements.Memory.HasValue)
            {
                // still enough if required memory is less than available
                enoughCapacity = enoughCapacity && requirements.Memory.Value <= nodeResource.Memory;
            }
            
            // Nvidia GPU is required
            if (requirements.NvidiaGpu.HasValue)
            {
                // still enough if required memory is less than available
                enoughCapacity = enoughCapacity && requirements.NvidiaGpu.Value <= nodeResource.NvidiaGpu;
            }
            
            // AMD GPU is required
            if (requirements.AmdGpu.HasValue)
            {
                // still enough if required memory is less than available
                enoughCapacity = enoughCapacity && requirements.AmdGpu.Value <= nodeResource.AmdGpu;
            }

            // if enough based on all resource requirement stop the process
            if (enoughCapacity)
            {
                capableNodes.Add(nodeResource);
            }
        }

        return capableNodes;
    }
    
    /// <summary>
    /// Ensure that there are no contradictions between parts of single resource requirement
    /// </summary>
    /// <param name="input">The input to validate</param>
    protected void ValidateSelfContradiction(JobRunManifestResourcesModel input)
    {
        // check if both Nvidia and AMD GPUs are requested
        if (input is { NvidiaGpu: not null, AmdGpu: not null })
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_RESOURCES, "You cannot request both Nvidia and AMD GPUs").AsException();
        }
    }

    /// <summary>
    /// Checks if resource requirements contains distributable unit value (CPU or GPU)
    /// </summary>
    /// <param name="resources">The resources</param>
    /// <returns></returns>
    protected bool HasDistributableValue(JobRunManifestResourcesModel resources)
    {
        return resources.Cpu is > 0 || resources.NvidiaGpu is > 0 || resources.AmdGpu is > 0;
    }
    
    /// <summary>
    /// Gets the total resources of the cluster
    /// </summary>
    /// <returns></returns>
    private ClusterTotalResourcesModel GetClusterTotal()
    {
        // total result
        var result = new ClusterTotalResourcesModel();
        
        // process nodes
        foreach (var node in this.readyNodes)
        {
            result.Cpu += node.Cpu;
            result.Memory += node.Memory;
            result.NvidiaGpu += node.NvidiaGpu ?? 0;
            result.AmdGpu += node.AmdGpu ?? 0;
        }
        
        // return result
        return result;
    }
}