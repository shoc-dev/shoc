using Microsoft.AspNetCore.DataProtection;

namespace Shoc.Registry.Services;

/// <summary>
/// The signing key protection provider
/// </summary>
public class SigningKeyProtectionProvider
{
    /// <summary>
    /// The signing key protection purpose
    /// </summary>
    private const string SIGNING_KEY_PROTECTION_PURPOSE = "signing-key";

    /// <summary>
    /// The data protection provider
    /// </summary>
    private readonly IDataProtectionProvider dataProtectionProvider;

    /// <summary>
    /// Creates new signing key protection provider
    /// </summary>
    /// <param name="dataProtectionProvider">The protection provider</param>
    public SigningKeyProtectionProvider(IDataProtectionProvider dataProtectionProvider)
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
        var baseProtector = this.dataProtectionProvider.CreateProtector(SIGNING_KEY_PROTECTION_PURPOSE);
        
        // if the tenant is defined use tenant specific protector
        return !string.IsNullOrWhiteSpace(workspaceId) ? baseProtector.CreateProtector(workspaceId) : baseProtector;
    }
}