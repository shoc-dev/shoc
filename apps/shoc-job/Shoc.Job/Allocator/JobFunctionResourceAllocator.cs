using System.Collections.Generic;
using System.Text.Json;
using Shoc.Job.Model.Cluster;
using Shoc.Job.Model.Job;

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
        output.Resources.Cpu ??= DEFAULT_FUNCTION_CPU;
        output.Resources.Memory ??= DEFAULT_FUNCTION_MEMORY;
        
        // validate self-consistency of the resources
        this.ValidateSelfContradiction(output.Resources);
        
        // validating total resource requirements
        this.ValidateResourcesRangeValidity(output.Resources);
        
        // validate if running job is feasible on the cluster
        this.ValidateAnyNodeFeasibility(output.Resources);

        // return modified output
        return output;
    }
}