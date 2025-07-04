using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Shoc.ApiCore.GrpcClient;
using Shoc.Cluster.Data;
using Shoc.Cluster.Model;
using Shoc.Cluster.Model.Cluster;
using Shoc.Core;

namespace Shoc.Cluster.Services;

/// <summary>
/// The cluster service
/// </summary>
public class ClusterService : ClusterServiceBase
{
    /// <summary>
    /// Creates new instance of registry service
    /// </summary>
    /// <param name="registryRepository">The registry repository</param>
    /// <param name="configurationProtectionProvider">The configuration protection provider</param>
    /// <param name="grpcClientProvider">The grpc client provider</param>
    public ClusterService(IClusterRepository registryRepository, ConfigurationProtectionProvider configurationProtectionProvider, IGrpcClientProvider grpcClientProvider) : 
        base(registryRepository, configurationProtectionProvider, grpcClientProvider)
    {
    }
    
    /// <summary>
    /// Gets all the objects
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<ClusterModel>> GetAll(string workspaceId)
    {
        // require the parent object
        await this.RequireWorkspace(workspaceId);
        
        return await this.clusterRepository.GetAll(workspaceId);
    }
    
    /// <summary>
    /// Gets all the extended objects
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<ClusterExtendedModel>> GetAllExtended(string workspaceId)
    {
        // require the parent object
        await this.RequireWorkspace(workspaceId);
        
        return await this.clusterRepository.GetAllExtended(workspaceId);
    }
    
    /// <summary>
    /// Gets all the extended objects
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<ClusterExtendedModel>> GetAllExtended()
    {   
        return await this.clusterRepository.GetAllExtended(string.Empty);
    }
    
    /// <summary>
    /// Get all referential values
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<ClusterReferentialValueModel>> GetAllReferentialValues(string workspaceId)
    {
        // require the parent object
        await this.RequireWorkspace(workspaceId);
        
        return await this.clusterRepository.GetAllReferentialValues(workspaceId);
    }
    
    /// <summary>
    /// Gets the object by id
    /// </summary>
    /// <returns></returns>
    public async Task<ClusterModel> GetById(string workspaceId, string id)
    {
        // require the parent object
        await this.RequireWorkspace(workspaceId);
        
        return await this.RequireClusterById(workspaceId, id);
    }
    
    /// <summary>
    /// Gets the object by id
    /// </summary>
    /// <returns></returns>
    public async Task<ClusterExtendedModel> GetExtendedById(string workspaceId, string id)
    {
        // require the parent object
        await this.RequireWorkspace(workspaceId);
        
        // try getting object by id
        var result = await this.clusterRepository.GetExtendedById(workspaceId, id);

        // make sure object exists
        if (result == null)
        {
            throw ErrorDefinition.NotFound().AsException();
        }

        // return the object
        return result;
    }
    
    /// <summary>
    /// Gets the object by workspace id and the name 
    /// </summary>
    /// <returns></returns>
    public async Task<ClusterModel> GetByName(string workspaceId, string name)
    {
        // make sure workspace and name are given
        if (string.IsNullOrWhiteSpace(workspaceId) || string.IsNullOrWhiteSpace(name))
        {
            throw ErrorDefinition.NotFound().AsException();
        }
        
        // require the parent object
        await this.RequireWorkspace(workspaceId);
        
        // try getting the result
        var result = await this.clusterRepository.GetByName(workspaceId, name);

        // check if object exists
        if (result == null)
        {
            throw ErrorDefinition.NotFound().AsException();
        }

        return result;
    } 
    
    /// <summary>
    /// Gets the object by workspace id and the name 
    /// </summary>
    /// <returns></returns>
    public async Task<ClusterExtendedModel> GetExtendedByName(string workspaceId, string name)
    {
        // make sure workspace and name are given
        if (string.IsNullOrWhiteSpace(workspaceId) || string.IsNullOrWhiteSpace(name))
        {
            throw ErrorDefinition.NotFound().AsException();
        }
        
        // require the parent object
        await this.RequireWorkspace(workspaceId);
        
        // try getting the result
        var result = await this.clusterRepository.GetExtendedByName(workspaceId, name);

        // check if object exists
        if (result == null)
        {
            throw ErrorDefinition.NotFound().AsException();
        }

        return result;
    } 
    
    /// <summary>
    /// Gets the object by id
    /// </summary>
    /// <returns></returns>
    public async Task<ClusterCountModel> CountAll(string workspaceId)
    {
        // require the parent object
        await this.RequireWorkspace(workspaceId);
        
        return await this.clusterRepository.CountAll(workspaceId);
    }
    
    /// <summary>
    /// Creates the object with given input
    /// </summary>
    /// <param name="workspaceId">The workspace id</param>
    /// <param name="input">The creation input</param>
    /// <returns></returns>
    public async Task<ClusterModel> Create(string workspaceId, ClusterCreateModel input)
    {
        // ensure referring to the correct object
        input.WorkspaceId = workspaceId;
        
        // active on creation
        input.Status = ClusterStatuses.ACTIVE;
        
        // empty configuration
        input.Configuration ??= string.Empty;

        // initialize the description
        input.Description ??= string.Empty;
        
        // truncate the description for safety
        input.Description = input.Description[..Math.Min(input.Description.Length, MAX_DESCRIPTION_LENGTH)];
        
        // require the parent object
        await this.RequireWorkspace(input.WorkspaceId); 
        
        // validate the name
        ValidateName(input.Name);
        
        // validate the provider
        ValidateType(input.Type);
        
        // validate the status
        ValidateStatus(input.Status);
        
        // try getting an object by name 
        var existingByName = await this.clusterRepository.GetByName(workspaceId, input.Name);
        
        // report error if object by name exists
        if (existingByName != null)
        {
            throw ErrorDefinition.Validation(ClusterErrors.EXISTING_NAME).AsException();
        }
        
        // create a protector
        var protector = this.configurationProtectionProvider.Create(input.WorkspaceId);

        // assign encrypted configuration
        input.Configuration = protector.Protect(input.Configuration);
        
        // create in the storage
        return await this.clusterRepository.Create(workspaceId, input);
    }

    /// <summary>
    /// Updates the object by id
    /// </summary>
    /// <param name="workspaceId">The workspace id</param>
    /// <param name="id">The object id</param>
    /// <param name="input">The update input</param>
    /// <returns></returns>
    public async Task<ClusterModel> UpdateById(string workspaceId, string id, ClusterUpdateModel input)
    {
        // make sure referring to the correct object
        input.WorkspaceId = workspaceId;
        input.Id = id;

        // initialize the description
        input.Description ??= string.Empty;
        
        // truncate the description for safety
        input.Description = input.Description[..Math.Min(input.Description.Length, MAX_DESCRIPTION_LENGTH)];
        
        // require the parent object
        await this.RequireWorkspace(workspaceId);
        
        // ensure object exists and throw otherwise
        var existing = await this.RequireClusterById(input.WorkspaceId, input.Id);
        
        // validate the name
        ValidateName(input.Name);
                
        // validate the status
        ValidateStatus(input.Status);
        
        // validate the type
        ValidateType(input.Type);
        
        // try getting an object by name
        var existingByName = await this.clusterRepository.GetByName(existing.WorkspaceId, input.Name);
        
        // if object exists but not referring to the current object then report name conflict
        if (existingByName != null && existingByName.Id != existing.Id)
        {
            throw ErrorDefinition.Validation(ClusterErrors.EXISTING_NAME).AsException();
        }

        // update in the storage
        return await this.clusterRepository.UpdateById(input.WorkspaceId, input.Id, input);
    }
    
    /// <summary>
    /// Updates the object configuration by id
    /// </summary>
    /// <param name="workspaceId">The workspace id</param>
    /// <param name="id">The object id</param>
    /// <param name="input">The update input</param>
    /// <returns></returns>
    public async Task<ClusterModel> UpdateConfigurationById(string workspaceId, string id, ClusterConfigurationUpdateModel input)
    {
        // make sure referring to the correct object
        input.WorkspaceId = workspaceId;
        input.Id = id;
        
        // ensure at least has empty string
        input.Configuration ??= string.Empty;
        
        // require the parent object
        await this.RequireWorkspace(workspaceId);
        
        // ensure object exists and throw otherwise
        await this.RequireClusterById(input.WorkspaceId, input.Id);

        // create a protector
        var protector = this.configurationProtectionProvider.Create(input.WorkspaceId);

        // assign encrypted configuration
        input.Configuration = protector.Protect(input.Configuration);
        
        // update in the storage
        return await this.clusterRepository.UpdateConfigurationById(input.WorkspaceId, input.Id, input);
    }
    
    /// <summary>
    /// Deletes the object by id
    /// </summary>
    /// <param name="workspaceId">The workspace id</param>
    /// <param name="id">The id of the object</param>
    /// <returns></returns>
    public async Task<ClusterModel> DeleteById(string workspaceId, string id)
    {
        // require the parent object
        await this.RequireWorkspace(workspaceId);
        
        // try deleting object by id
        var result = await this.clusterRepository.DeleteById(workspaceId, id);

        // make sure object exists
        if (result == null)
        {
            throw ErrorDefinition.NotFound().AsException();
        }

        // return the object
        return result;
    }
}