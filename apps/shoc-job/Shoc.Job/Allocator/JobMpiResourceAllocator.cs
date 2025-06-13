using System.Collections.Generic;
using System.Text.Json;
using Shoc.Job.Model.Cluster;
using Shoc.Job.Model.Job;

namespace Shoc.Job.Allocator;

/// <summary>
/// The MPI job resource allocator
/// </summary>
public class JobMpiResourceAllocator : JobResourceAllocatorBase
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
    public JobMpiResourceAllocator(JobResourceFormatter jobResourceFormatter, List<ClusterNodeResourcesModel> nodes) : base(jobResourceFormatter, nodes)
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

        // initialize mpi spec
        output.Spec.Mpi ??= new JobRunManifestSpecMpiModel();
        output.Spec.Mpi.Workers ??= new JobRunManifestSpecMpiWorkersModel();
        
        // initialize default values
        output.Resources ??= new JobRunManifestResourcesModel();
        output.Resources.Cpu ??= $"{DEFAULT_FUNCTION_CPU}m";
        output.Resources.Memory ??= DEFAULT_FUNCTION_MEMORY.ToString();

        // parse total resource requirements
        var total = this.jobResourceFormatter.FromManifest(output.Resources);
        
        // validating total resource requirements
        this.ValidateResources(total);

        // return modified output
        return output;
    }
    
}