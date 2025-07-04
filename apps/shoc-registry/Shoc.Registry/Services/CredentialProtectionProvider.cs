using Microsoft.AspNetCore.DataProtection;

namespace Shoc.Registry.Services;

/// <summary>
/// The credential protection provider
/// </summary>
public class CredentialProtectionProvider
{
    /// <summary>
    /// The password protection purpose
    /// </summary>
    private const string PASSWORD_PROTECTION_PURPOSE = "password_protection";

    /// <summary>
    /// The data protection provider
    /// </summary>
    private readonly IDataProtectionProvider dataProtectionProvider;

    /// <summary>
    /// Creates new credential protection provider
    /// </summary>
    /// <param name="dataProtectionProvider">The protection provider</param>
    public CredentialProtectionProvider(IDataProtectionProvider dataProtectionProvider)
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
        var baseProtector = this.dataProtectionProvider.CreateProtector(PASSWORD_PROTECTION_PURPOSE);
        
        // if the tenant is defined use tenant specific protector
        return !string.IsNullOrWhiteSpace(workspaceId) ? baseProtector.CreateProtector(workspaceId) : baseProtector;
    }
}