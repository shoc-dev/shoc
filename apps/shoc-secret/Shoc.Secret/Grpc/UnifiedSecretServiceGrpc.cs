using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.DataProtection;
using Shoc.ApiCore.Access;
using Shoc.Core.OpenId;
using Shoc.Secret.Grpc.Secrets;
using Shoc.Secret.Services;

namespace Shoc.Secret.Grpc;

/// <summary>
/// The grpc service implementation
/// </summary>
[BearerOnly]
public class UnifiedSecretServiceGrpc : Secrets.UnifiedSecretServiceGrpc.UnifiedSecretServiceGrpcBase
{
    /// <summary>
    /// The access authorization service
    /// </summary>
    private readonly IAccessAuthorization accessAuthorization;

    /// <summary>
    /// The unified secret service
    /// </summary>
    private readonly UnifiedSecretService secretService;

    /// <summary>
    /// The secret protection provider
    /// </summary>
    private readonly SecretProtectionProvider secretProtectionProvider;

    /// <summary>
    /// The user secret protection provider
    /// </summary>
    private readonly UserSecretProtectionProvider userSecretProtectionProvider;

    /// <summary>
    /// The secret grpc mapper
    /// </summary>
    private readonly UnifiedSecretGrpcMapper secretGrpcMapper;

    /// <summary>
    /// Creates new instance of the grpc service
    /// </summary>
    /// <param name="accessAuthorization">The access authorization service</param>
    /// <param name="secretService">The unified secret service</param>
    /// <param name="secretProtectionProvider">The secret protection provider</param>
    /// <param name="userSecretProtectionProvider">The user secret protection provider</param>
    /// <param name="secretGrpcMapper">The secret grpc mapper</param>
    public UnifiedSecretServiceGrpc(IAccessAuthorization accessAuthorization, UnifiedSecretService secretService, SecretProtectionProvider secretProtectionProvider, UserSecretProtectionProvider userSecretProtectionProvider, UnifiedSecretGrpcMapper secretGrpcMapper)
    {
        this.accessAuthorization = accessAuthorization;
        this.secretService = secretService;
        this.secretProtectionProvider = secretProtectionProvider;
        this.userSecretProtectionProvider = userSecretProtectionProvider;
        this.secretGrpcMapper = secretGrpcMapper;
    }

    /// <summary>
    /// Gets the unified secrets by names
    /// </summary>
    /// <param name="request">The request object</param>
    /// <param name="context">The context object</param>
    /// <returns></returns>
    public override async Task<GetUnifiedSecretsResponse> GetByNames(GetUnifiedSecretsByNameRequest request, ServerCallContext context)
    {
        // ensure authorization
        await this.accessAuthorization.RequireScopesAll(context.GetHttpContext(), new []{ KnownScopes.SVC });

        // the list of items matching given names both for user-secrets and workspace-secrets
        var items = await this.secretService.GetByNames(request.WorkspaceId, request.UserId, request.Names);

        // build the result 
        var result = new GetUnifiedSecretsResponse();

        // the secret provider
        var secretProtector = secretProtectionProvider.Create(request.WorkspaceId);
        
        // the user secret provider
        var userSecretProtector = userSecretProtectionProvider.Create(request.WorkspaceId);
        
        // add resulting secrets mapped to grpc model
        result.Secrets.AddRange(items.Select(item =>
        {
            // map the item
            var mapped = this.secretGrpcMapper.Map(item);

            // choose the protector based on card
            var protector = mapped.Kind == UnifiedSecretKind.Workspace ? secretProtector : userSecretProtector;

            // if plain values are requested and the value is encrypted decrypt and override the value
            if (request.Plain && item.Encrypted)
            {
                mapped.Value = protector.Unprotect(mapped.Value);
            }
            
            // return mapped value
            return mapped;
        }));

        return result;
    }
}