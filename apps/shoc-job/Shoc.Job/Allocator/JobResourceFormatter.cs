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
    
    /// <summary>
    /// Parses the given resource strings into valid resource quantities
    /// </summary>
    /// <param name="resources">The resources to parse</param>
    /// <returns></returns>
    public JobTaskResourcesModel FromManifest(JobRunManifestResourcesModel resources)
    {
        // no resources to map
        if (resources == null)
        {
            return null;
        }
        
        return new JobTaskResourcesModel
        {
            Cpu = this.resourceParser.ParseToMillicores(resources.Cpu),
            Memory = this.resourceParser.ParseToBytes(resources.Memory),
            NvidiaGpu = this.resourceParser.ParseToGpu(resources.NvidiaGpu),
            AmdGpu = this.resourceParser.ParseToGpu(resources.AmdGpu)
        };
    }
    
    /// <summary>
    /// Formats the task resources into manifest format
    /// </summary>
    /// <param name="resources">The resources to parse</param>
    /// <returns></returns>
    public JobRunManifestResourcesModel ToManifest(JobTaskResourcesModel resources)
    {
        // no resources to map
        if (resources == null)
        {
            return null;
        }
        
        return new JobRunManifestResourcesModel
        {
            Cpu = resources.Cpu.HasValue ? $"{resources.Cpu}m" : null,
            Memory = resources.Memory?.ToString(),
            NvidiaGpu = resources.NvidiaGpu?.ToString(),
            AmdGpu = resources.AmdGpu?.ToString()
        };
    }
}