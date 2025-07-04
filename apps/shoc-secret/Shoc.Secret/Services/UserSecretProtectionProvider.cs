using Microsoft.AspNetCore.DataProtection;

namespace Shoc.Secret.Services;

/// <summary>
/// The protection provider
/// </summary>
public class UserSecretProtectionProvider
{
    /// <summary>
    /// The protection purpose
    /// </summary>
    private const string USER_SECRET_VALUE_PURPOSE = "user-secret-value";

    /// <summary>
    /// The data protection provider
    /// </summary>
    private readonly IDataProtectionProvider dataProtectionProvider;

    /// <summary>
    /// Creates new protection provider
    /// </summary>
    /// <param name="dataProtectionProvider">The protection provider</param>
    public UserSecretProtectionProvider(IDataProtectionProvider dataProtectionProvider)
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
        var baseProtector = this.dataProtectionProvider.CreateProtector(USER_SECRET_VALUE_PURPOSE);
        
        // if the tenant is defined use tenant specific protector
        return !string.IsNullOrWhiteSpace(workspaceId) ? baseProtector.CreateProtector(workspaceId) : baseProtector;
    }
}