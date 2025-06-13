using System.Collections.Generic;
using System.Text.Json;
using Shoc.Core;
using Shoc.Job.Model;
using Shoc.Job.Model.Cluster;
using Shoc.Job.Model.Job;
using Shoc.Job.Model.JobTask;

namespace Shoc.Job.Allocator;

/// <summary>
/// The function job resource allocator
/// </summary>
public class JobFunctionResourceAllocator : JobResourceAllocatorBase
{
    /// <summary>
    /// The default CPU allocation for function job 100 millicores
    /// </summary>
    public const long DEFAULT_FUNCTION_CPU = 100;
    
    /// <summary>
    /// The default Memory allocation for function job 256Mi
    /// </summary>
    public const long DEFAULT_FUNCTION_MEMORY = 268435456;

    /// <summary>
    /// The base service for resource handling 
    /// </summary>
    /// <param name="jobResourceFormatter">The resource formatter</param>
    /// <param name="nodes">The nodes</param>
    public JobFunctionResourceAllocator(JobResourceFormatter jobResourceFormatter, List<ClusterNodeResourcesModel> nodes) : base(jobResourceFormatter, nodes)
    {
    }

    /// <summary>
    /// Returns new manifest instance with enriched resource definitions
    /// </summary>
    /// <param name="input">The input</param>
    /// <returns></returns>
    public override JobRunManifestModel Allocate(JobRunManifestModel input)
    {
        // make a copy for output
        var output = JsonSerializer.Deserialize<JobRunManifestModel>(JsonSerializer.Serialize(input));
        
        // initialize default values
        output.Resources ??= new JobRunManifestResourcesModel();
        output.Resources.Cpu ??= $"{DEFAULT_FUNCTION_CPU}m";
        output.Resources.Memory ??= DEFAULT_FUNCTION_MEMORY.ToString();

        // parse total resource requirements
        var total = this.jobResourceFormatter.FromManifest(output.Resources);
        
        // validating total resource requirements
        this.ValidateResources(total);
        
        // validate if running job is feasible on the cluster
        this.ValidateClusterFeasibility(nodes, total);

        // return modified output
        return output;
    }
    
    /// <summary>
    /// Validates the cluster resources for function task
    /// </summary>
    /// <param name="nodeResources">The node resources</param>
    /// <param name="requirements">The resources to validate against</param>
    private void ValidateClusterFeasibility(IEnumerable<ClusterNodeResourcesModel> nodeResources, JobTaskResourcesModel requirements)
    {
        // count capable nodes
        var capableNodes = this.GetCapableNodes(nodeResources, requirements).Count;

        // if there are no capable nodes to take one task, report not feasible
        if (capableNodes == 0)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_RESOURCES, "None of the nodes has enough allocatable resources").AsException();
        }
    }
}