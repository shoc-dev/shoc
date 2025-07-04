using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Shoc.Core;
using Shoc.Secret.Data;
using Shoc.Secret.Model;
using Shoc.Secret.Model.UserSecret;

namespace Shoc.Secret.Services;

/// <summary>
/// The user secret service
/// </summary>
public class UserSecretService : UserSecretServiceBase
{
    /// <summary>
    /// Creates a new instance of the service
    /// </summary>
    /// <param name="userSecretRepository">The repository</param>
    /// <param name="protectionProvider">The protection provider</param>
    /// <param name="validationService">The validation service</param>
    public UserSecretService(IUserSecretRepository userSecretRepository, UserSecretProtectionProvider protectionProvider, SecretValidationService validationService) :
        base(userSecretRepository, protectionProvider, validationService)
    {
    }

    /// <summary>
    /// Gets all the objects
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<UserSecretModel>> GetAll(string workspaceId, string userId)
    {
        // require the parent object
        await this.validationService.RequireWorkspace(workspaceId);

        // require the correct user in the workspace
        await this.validationService.RequireMembership(workspaceId, userId);

        // get from the storage
        return await this.userSecretRepository.GetAll(workspaceId, userId);
    }

    /// <summary>
    /// Gets all the extended objects
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<UserSecretExtendedModel>> GetAllExtended(string workspaceId, string userId)
    {
        // require the parent object
        await this.validationService.RequireWorkspace(workspaceId);

        // require the correct user in the workspace
        await this.validationService.RequireMembership(workspaceId, userId);
        
        // get from the storage
        return await this.userSecretRepository.GetAllExtended(workspaceId, userId);
    }

    /// <summary>
    /// Gets the object by id
    /// </summary>
    /// <returns></returns>
    public async Task<UserSecretModel> GetById(string workspaceId, string userId, string id)
    {
        // require the parent object
        await this.validationService.RequireWorkspace(workspaceId);

        // require the correct user in the workspace
        await this.validationService.RequireMembership(workspaceId, userId);
        
        // get from the storage
        return await this.userSecretRepository.GetById(workspaceId, userId, id);
    }

    /// <summary>
    /// Count objects by workspace id
    /// </summary>
    /// <param name="workspaceId">The workspace id</param>
    /// <param name="userId">The user id</param>
    /// <returns></returns>
    public async Task<UserSecretCountModel> CountAll(string workspaceId, string userId)
    {
        // require the parent object
        await this.validationService.RequireWorkspace(workspaceId);
        
        // require the correct user in the workspace
        await this.validationService.RequireMembership(workspaceId, userId);

        // get from the storage
        return await this.userSecretRepository.CountAll(workspaceId, userId);
    }
    
    /// <summary>
    /// Creates the object with given input
    /// </summary>
    /// <param name="workspaceId">The workspace id</param>
    /// <param name="userId">The user id</param>
    /// <param name="input">The creation input</param>
    /// <returns></returns>
    public async Task<UserSecretModel> Create(string workspaceId, string userId, UserSecretCreateModel input)
    {
        // ensure referring to the correct object
        input.WorkspaceId = workspaceId;
        input.UserId = userId;
        
        // initialize the description
        input.Description ??= string.Empty;
        
        // value should be at least empty string
        input.Value ??= string.Empty;

        // require the parent object
        await this.validationService.RequireWorkspace(workspaceId);
        
        // require the correct user in the workspace
        await this.validationService.RequireMembership(workspaceId, userId);
        
        // validate name
        this.validationService.ValidateName(input.Name);
        
        // validate description
        this.validationService.ValidateDescription(input.Description);

        // try getting object by name
        var existing = await this.userSecretRepository.GetByName(workspaceId, userId, input.Name);

        // if object by name exists
        if (existing != null)
        {
            throw ErrorDefinition.Validation(SecretErrors.EXISTING_NAME).AsException();
        }
        
        // create a protector
        var protector = this.protectionProvider.Create(workspaceId);
        
        // encrypt if needed
        input.Value = input.Encrypted ? protector.Protect(input.Value) : input.Value;
        
        // create object in the storage
        return await this.userSecretRepository.Create(workspaceId, userId, input);
    }

    /// <summary>
    /// Updates the object by id
    /// </summary>
    /// <param name="workspaceId">The workspace id</param>
    /// <param name="userId">The user id</param>
    /// <param name="id">The object id</param>
    /// <param name="input">The update input</param>
    /// <returns></returns>
    public async Task<UserSecretModel> UpdateById(string workspaceId, string userId, string id, UserSecretUpdateModel input)
    {
        // make sure referring to the correct object
        input.WorkspaceId = workspaceId;
        input.UserId = userId;
        input.Id = id;

        // initialize the description
        input.Description ??= string.Empty;
        
        // require the parent object
        await this.validationService.RequireWorkspace(workspaceId);
        
        // require the correct user in the workspace
        await this.validationService.RequireMembership(workspaceId, userId);
        
        // ensure object exists
        var existing = await this.RequireById(workspaceId, userId, id);
        
        // validate name
        this.validationService.ValidateName(input.Name);
        
        // validate description
        this.validationService.ValidateDescription(input.Description);

        // try getting an object by name
        var existingByName = await this.userSecretRepository.GetByName(existing.WorkspaceId, existing.UserId, input.Name);
        
        // if object exists but not referring to the current object then report name conflict
        if (existingByName != null && existingByName.Id != existing.Id)
        {
            throw ErrorDefinition.Validation(SecretErrors.EXISTING_NAME).AsException();
        }

        // update in the storage
        return await this.userSecretRepository.UpdateById(workspaceId, userId, id, input);
    }

    /// <summary>
    /// Updates the object value by id
    /// </summary>
    /// <param name="workspaceId">The workspace id</param>
    /// <param name="userId">The user id</param>
    /// <param name="id">The object id</param>
    /// <param name="input">The update input</param>
    /// <returns></returns>
    public async Task<UserSecretModel> UpdateValueById(string workspaceId, string userId, string id, UserSecretValueUpdateModel input)
    {
        // make sure referring to the correct object
        input.WorkspaceId = workspaceId;
        input.UserId = userId;
        input.Id = id;
        
        // require the parent object
        await this.validationService.RequireWorkspace(workspaceId);
        
        // require the correct user in the workspace
        await this.validationService.RequireMembership(workspaceId, userId);
        
        // ensure object exists
        await this.RequireById(workspaceId, userId, id);
        
        // create a protector
        var protector = this.protectionProvider.Create(workspaceId);
        
        // encrypt if needed
        input.Value = input.Encrypted ? protector.Protect(input.Value) : input.Value;
        
        // update in the storage
        return await this.userSecretRepository.UpdateValueById(workspaceId, userId, id, input);
    }

    /// <summary>
    /// Deletes the object by id
    /// </summary>
    /// <param name="workspaceId">The workspace id</param>
    /// <param name="userId">The user id</param>
    /// <param name="id">The id of the object</param>
    /// <returns></returns>
    public async Task<UserSecretModel> DeleteById(string workspaceId, string userId, string id)
    {
        // require the parent object
        await this.validationService.RequireWorkspace(workspaceId);
        
        // require the correct user in the workspace
        await this.validationService.RequireMembership(workspaceId, userId);
        
        // try deleting object by id
        var result = await this.userSecretRepository.DeleteById(workspaceId, userId, id);

        // make sure object exists
        if (result == null)
        {
            throw ErrorDefinition.NotFound().AsException();
        }

        // return the object
        return result;
    }
}