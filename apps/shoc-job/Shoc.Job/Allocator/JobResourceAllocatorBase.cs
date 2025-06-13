using System.Collections.Generic;
using Shoc.Core;
using Shoc.Job.Model;
using Shoc.Job.Model.Cluster;
using Shoc.Job.Model.Job;
using Shoc.Job.Model.JobTask;

namespace Shoc.Job.Allocator;

/// <summary>
/// The resource handler base service
/// </summary>
public abstract class JobResourceAllocatorBase
{
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
    /// The resource formatter
    /// </summary>
    protected readonly JobResourceFormatter jobResourceFormatter;

    /// <summary>
    /// The nodes to allocate against
    /// </summary>
    protected readonly List<ClusterNodeResourcesModel> nodes;

    /// <summary>
    /// The base service for resource handling 
    /// </summary>
    /// <param name="jobResourceFormatter">The resource formatter</param>
    /// <param name="nodes">The nodes</param>
    protected JobResourceAllocatorBase(JobResourceFormatter jobResourceFormatter, List<ClusterNodeResourcesModel> nodes)
    {
        this.jobResourceFormatter = jobResourceFormatter;
        this.nodes = nodes;
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
    protected void ValidateResources(JobTaskResourcesModel input)
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
        if (input.NvidiaGpu <= 0)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_RESOURCES, "Requested GPU should be positive").AsException();
        }
        
        // Nvidia GPU should be within limit
        if (input.NvidiaGpu > MAX_JOB_REQUESTED_NVIDIA_GPU)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_RESOURCES, "Too much GPU requested").AsException();
        }
    }
    
    /// <summary>
    /// Gets all the nodes capable to fit given requirements
    /// </summary>
    /// <param name="allNodes">The node resources</param>
    /// <param name="requirements">The resources to validate against</param>
    protected List<ClusterNodeResourcesModel> GetCapableNodes(IEnumerable<ClusterNodeResourcesModel> allNodes, JobTaskResourcesModel requirements)
    {
        // capable nodes
        var capableNodes = new List<ClusterNodeResourcesModel>();
        
        // check each node
        foreach (var nodeResource in allNodes)
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
}