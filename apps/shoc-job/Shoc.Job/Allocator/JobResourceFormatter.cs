using Shoc.Core.Kubernetes;
using Shoc.Job.Model.Job;
using Shoc.Job.Model.JobTask;

namespace Shoc.Job.Allocator;

/// <summary>
/// The job resource formatter service
/// </summary>
public class JobResourceFormatter
{
    /// <summary>
    /// The resource parser instance
    /// </summary>
    private readonly ResourceParser resourceParser;

    /// <summary>
    /// The job resource formatting service
    /// </summary>
    /// <param name="resourceParser">The resource parser</param>
    public JobResourceFormatter(ResourceParser resourceParser)
    {
        this.resourceParser = resourceParser;
    }
}