using Microsoft.AspNetCore.DataProtection;

namespace Shoc.Secret.Services;

/// <summary>
/// The protection provider
/// </summary>
public class SecretProtectionProvider
{
    /// <summary>
    /// The protection purpose
    /// </summary>
    private const string SECRET_VALUE_PURPOSE = "secret-value";

    /// <summary>
    /// The data protection provider
    /// </summary>
    private readonly IDataProtectionProvider dataProtectionProvider;

    /// <summary>
    /// Creates new protection provider
    /// </summary>
    /// <param name="dataProtectionProvider">The protection provider</param>
    public SecretProtectionProvider(IDataProtectionProvider dataProtectionProvider)
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
        var baseProtector = this.dataProtectionProvider.CreateProtector(SECRET_VALUE_PURPOSE);
        
        // if the tenant is defined use tenant specific protector
        return !string.IsNullOrWhiteSpace(workspaceId) ? baseProtector.CreateProtector(workspaceId) : baseProtector;
    }
}