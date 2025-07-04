using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.DataProtection;
using Shoc.ApiCore.Access;
using Shoc.Core.OpenId;
using Shoc.Registry.Grpc.Registries;
using Shoc.Registry.Model.Credential;
using Shoc.Registry.Services;

namespace Shoc.Registry.Grpc;

/// <summary>
/// The Grpc service implementation
/// </summary>
[BearerOnly]
public class RegistryPlainCredentialServiceGrpc : Registries.RegistryPlainCredentialServiceGrpc.RegistryPlainCredentialServiceGrpcBase
{
    /// <summary>
    /// The access authorization service
    /// </summary>
    private readonly IAccessAuthorization accessAuthorization;

    /// <summary>
    /// The registry credential service
    /// </summary>
    private readonly RegistryCredentialService registryCredentialService;
    
    /// <summary>
    /// The credential protection provider
    /// </summary>
    private readonly CredentialProtectionProvider credentialProtectionProvider;

    /// <summary>
    /// Creates new instance of the service
    /// </summary>
    /// <param name="accessAuthorization">The access authorizatoin</param>
    /// <param name="registryCredentialService">The registry credential service</param>
    /// <param name="credentialProtectionProvider">The credential protection provider</param>
    public RegistryPlainCredentialServiceGrpc(IAccessAuthorization accessAuthorization, RegistryCredentialService registryCredentialService, CredentialProtectionProvider credentialProtectionProvider)
    {
        this.accessAuthorization = accessAuthorization;
        this.registryCredentialService = registryCredentialService;
        this.credentialProtectionProvider = credentialProtectionProvider;
    }

    /// <summary>
    /// Gets the registry plain credential for push operations
    /// </summary>
    /// <param name="request">The request</param>
    /// <param name="context">The context</param>
    /// <returns></returns>
    public override async Task<GetRegistryPlainCredentialResponse> GetPushCredentialOrCreate(GetRegistryPlainCredentialRequest request, ServerCallContext context)
    {
        // ensure authorization
        await this.accessAuthorization.RequireScopesAll(context.GetHttpContext(), new []{ KnownScopes.SVC });

        // create a protector
        var protector = this.credentialProtectionProvider.Create(request.WorkspaceId);
        
        // the result of the operation
        var result =
            await this.registryCredentialService.GetOrCreatePushCredential(request.RegistryId, request.WorkspaceId,
                request.UserId);

        return new GetRegistryPlainCredentialResponse
        {
            Credential = Map(result, protector)
        };
    }

    /// <summary>
    /// Gets the registry plain credential for pull operation
    /// </summary>
    /// <param name="request">The request</param>
    /// <param name="context">The context</param>
    /// <returns></returns>
    public override async Task<GetRegistryPlainCredentialResponse> GetPullCredentialOrCreate(GetRegistryPlainCredentialRequest request, ServerCallContext context)
    {
        // ensure authorization
        await this.accessAuthorization.RequireScopesAll(context.GetHttpContext(), new []{ KnownScopes.SVC });

        // create a protector
        var protector = this.credentialProtectionProvider.Create(request.WorkspaceId);

        // the result of the operation
        var result = await this.registryCredentialService.GetOrCreatePullCredential(request.RegistryId, request.WorkspaceId, request.UserId);
        
        return new GetRegistryPlainCredentialResponse
        {
            Credential = Map(result, protector)
        };
    }

    /// <summary>
    /// Maps to the grpc representation
    /// </summary>
    /// <param name="credential">The credential to map</param>
    /// <param name="protector">The protector</param>
    /// <returns></returns>
    private static RegistryPlainCredentialGrpcModel Map(RegistryCredentialModel credential, IDataProtector protector)
    {
        return new RegistryPlainCredentialGrpcModel
        {
            Id = credential.Id,
            RegistryId = credential.RegistryId,
            WorkspaceId = credential.WorkspaceId ?? string.Empty,
            UserId = credential.UserId ?? string.Empty,
            Source = credential.Source,
            Username = credential.Username,
            PasswordPlain = protector.Unprotect(credential.PasswordEncrypted),
            Email = credential.Email ?? string.Empty,
            PushAllowed = credential.PushAllowed,
            PullAllowed = credential.PullAllowed
        };
    }
}