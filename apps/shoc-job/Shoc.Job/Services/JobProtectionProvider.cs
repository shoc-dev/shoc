using Microsoft.AspNetCore.DataProtection;

namespace Shoc.Job.Services;

/// <summary>
/// The protection provider
/// </summary>
public class JobProtectionProvider
{
    /// <summary>
    /// The protection purpose
    /// </summary>
    private const string JOB_DATA_PURPOSE = "job-data";

    /// <summary>
    /// The data protection provider
    /// </summary>
    private readonly IDataProtectionProvider dataProtectionProvider;

    /// <summary>
    /// Creates new protection provider
    /// </summary>
    /// <param name="dataProtectionProvider">The protection provider</param>
    public JobProtectionProvider(IDataProtectionProvider dataProtectionProvider)
    {
        this.dataProtectionProvider = dataProtectionProvider;
    }

    /// <summary>
    /// Creates a new data protector
    /// </summary>
    /// <returns></returns>
    public IDataProtector Create(string workspaceId)
    {
        // create a base protector
        var baseProtector = this.dataProtectionProvider.CreateProtector(JOB_DATA_PURPOSE);
        
        // if the tenant is defined use tenant specific protector
        return !string.IsNullOrWhiteSpace(workspaceId) ? baseProtector.CreateProtector(workspaceId) : baseProtector;
    }
}