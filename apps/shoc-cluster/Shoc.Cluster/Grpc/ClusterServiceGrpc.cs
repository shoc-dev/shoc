using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.DataProtection;
using Shoc.ApiCore.Access;
using Shoc.Cluster.Grpc.Clusters;
using Shoc.Cluster.Services;
using Shoc.Core.OpenId;

namespace Shoc.Cluster.Grpc;

/// <summary>
/// The grpc service implementation
/// </summary>
[BearerOnly]
public class ClusterServiceGrpc : Clusters.ClusterServiceGrpc.ClusterServiceGrpcBase
{
    /// <summary>
    /// The access authorization service
    /// </summary>
    private readonly IAccessAuthorization accessAuthorization;

    /// <summary>
    /// The cluster service
    /// </summary>
    private readonly ClusterService clusterService;

    /// <summary>
    /// The protection provider
    /// </summary>
    private readonly ConfigurationProtectionProvider protectionProvider;

    /// <summary>
    /// The mapper instance
    /// </summary>
    private readonly ClusterGrpcMapper mapper;

    /// <summary>
    /// Creates new instance of the service
    /// </summary>
    /// <param name="accessAuthorization">The access authorization</param>
    /// <param name="clusterService">The cluster service</param>
    /// <param name="protectionProvider">The protection provider</param>
    /// <param name="mapper">The mapper instance</param>
    public ClusterServiceGrpc(IAccessAuthorization accessAuthorization, ClusterService clusterService, ConfigurationProtectionProvider protectionProvider, ClusterGrpcMapper mapper)
    {
        this.accessAuthorization = accessAuthorization;
        this.clusterService = clusterService;
        this.protectionProvider = protectionProvider;
        this.mapper = mapper;
    }

    /// <summary>
    /// Gets the cluster by id
    /// </summary>
    /// <param name="request">The request object</param>
    /// <param name="context">The context object</param>
    /// <returns></returns>
    public override async Task<GetClusterResponse> GetById(GetClusterByIdRequest request, ServerCallContext context)
    {
        // ensure authorization
        await this.accessAuthorization.RequireScopesAll(context.GetHttpContext(), new []{ KnownScopes.SVC });
        
        // gets the object by id
        var result = await this.clusterService.GetById(request.WorkspaceId, request.Id);
        
        // if plain configuration is requested then decrypt before returning
        if (request.Plain)
        {
            // create protector instance
            var protector = this.protectionProvider.Create(request.WorkspaceId);

            // decrypt configuration and override existing encrypted field
            result.Configuration = protector.Unprotect(result.Configuration);
        }
        
        // build and return the result
        return new GetClusterResponse
        {
            Cluster = this.mapper.Map(result)
        };
    }
}