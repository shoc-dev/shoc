using System.Collections.Generic;
using Shoc.Core;
using Shoc.Job.Model;
using Shoc.Job.Model.Cluster;
using Shoc.Job.Model.JobTask;

namespace Shoc.Job.Allocator;

/// <summary>
/// The resource allocator factory
/// </summary>
public class JobResourceAllocatorFactory
{
    /// <summary>
    /// The resource formatter
    /// </summary>
    private readonly JobResourceFormatter resourceFormatter;

    /// <summary>
    /// Creates new instance of the factory
    /// </summary>
    /// <param name="resourceFormatter">The resource formatter</param>
    public JobResourceAllocatorFactory(JobResourceFormatter resourceFormatter)
    {
        this.resourceFormatter = resourceFormatter;
    }

    /// <summary>
    /// Creates an allocator based on the task type
    /// </summary>
    /// <param name="type">The type of the task</param>
    /// <param name="nodes">The list of nodes in the cluster</param>
    /// <returns></returns>
    public JobResourceAllocatorBase Create(string type, List<ClusterNodeResourcesModel> nodes)
    {
        return type switch
        {
            JobTaskTypes.FUNCTION => new JobFunctionResourceAllocator(this.resourceFormatter, nodes),
            _ => throw ErrorDefinition.Validation(JobErrors.INVALID_RUNTIME_TYPE, $"The '{type}' jobs does not support resource allocation").AsException()
        };
    }
}