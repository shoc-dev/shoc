using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Shoc.Core;
using Shoc.Secret.Data;
using Shoc.Secret.Model;
using Shoc.Secret.Model.Secret;

namespace Shoc.Secret.Services;

/// <summary>
/// The secret service
/// </summary>
public class SecretService : SecretServiceBase
{
    /// <summary>
    /// Creates a new instance of the service
    /// </summary>
    /// <param name="secretRepository">The repository</param>
    /// <param name="protectionProvider">The protection provider</param>
    /// <param name="validationService">The validation service</param>
    public SecretService(ISecretRepository secretRepository, SecretProtectionProvider protectionProvider, SecretValidationService validationService) :
        base(secretRepository, protectionProvider, validationService)
    {
    }

    /// <summary>
    /// Gets all the objects
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<SecretModel>> GetAll(string workspaceId)
    {
        // require the parent object
        await this.validationService.RequireWorkspace(workspaceId);

        // get from the storage
        return await this.secretRepository.GetAll(workspaceId);
    }

    /// <summary>
    /// Gets all the extended objects
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<SecretExtendedModel>> GetAllExtended(string workspaceId)
    {
        // require the parent object
        await this.validationService.RequireWorkspace(workspaceId);

        // get from the storage
        return await this.secretRepository.GetAllExtended(workspaceId);
    }

    /// <summary>
    /// Gets the object by id
    /// </summary>
    /// <returns></returns>
    public async Task<SecretModel> GetById(string workspaceId, string id)
    {
        // require the parent object
        await this.validationService.RequireWorkspace(workspaceId);

        // get from the storage
        return await this.secretRepository.GetById(workspaceId, id);
    }

    /// <summary>
    /// Count objects by workspace id
    /// </summary>
    /// <param name="workspaceId">The workspace id</param>
    /// <returns></returns>
    public async Task<SecretCountModel> CountAll(string workspaceId)
    {
        // require the parent object
        await this.validationService.RequireWorkspace(workspaceId);

        // get from the storage
        return await this.secretRepository.CountAll(workspaceId);
    }
    
    /// <summary>
    /// Creates the object with given input
    /// </summary>
    /// <param name="workspaceId">The workspace id</param>
    /// <param name="input">The creation input</param>
    /// <returns></returns>
    public async Task<SecretModel> Create(string workspaceId, SecretCreateModel input)
    {
        // ensure referring to the correct object
        input.WorkspaceId = workspaceId;
        
        // initialize the description
        input.Description ??= string.Empty;
        
        // value should be at least empty string
        input.Value ??= string.Empty;

        // require the parent object
        await this.validationService.RequireWorkspace(workspaceId);
        
        // validate name
        this.validationService.ValidateName(input.Name);
        
        // validate description
        this.validationService.ValidateDescription(input.Description);

        // try getting object by name
        var existing = await this.secretRepository.GetByName(workspaceId, input.Name);

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
        return await this.secretRepository.Create(workspaceId, input);
    }

    /// <summary>
    /// Updates the object by id
    /// </summary>
    /// <param name="workspaceId">The workspace id</param>
    /// <param name="id">The object id</param>
    /// <param name="input">The update input</param>
    /// <returns></returns>
    public async Task<SecretModel> UpdateById(string workspaceId, string id, SecretUpdateModel input)
    {
        // make sure referring to the correct object
        input.WorkspaceId = workspaceId;
        input.Id = id;

        // initialize the description
        input.Description ??= string.Empty;
        
        // require the parent object
        await this.validationService.RequireWorkspace(workspaceId);
        
        // ensure object exists
        var existing = await this.RequireById(workspaceId, id);
        
        // validate name
        this.validationService.ValidateName(input.Name);
        
        // validate description
        this.validationService.ValidateDescription(input.Description);

        // try getting an object by name
        var existingByName = await this.secretRepository.GetByName(existing.WorkspaceId, input.Name);
        
        // if object exists but not referring to the current object then report name conflict
        if (existingByName != null && existingByName.Id != existing.Id)
        {
            throw ErrorDefinition.Validation(SecretErrors.EXISTING_NAME).AsException();
        }

        // update in the storage
        return await this.secretRepository.UpdateById(workspaceId, id, input);
    }

    /// <summary>
    /// Updates the object value by id
    /// </summary>
    /// <param name="workspaceId">The workspace id</param>
    /// <param name="id">The object id</param>
    /// <param name="input">The update input</param>
    /// <returns></returns>
    public async Task<SecretModel> UpdateValueById(string workspaceId, string id, SecretValueUpdateModel input)
    {
        // make sure referring to the correct object
        input.WorkspaceId = workspaceId;
        input.Id = id;
        
        // require the parent object
        await this.validationService.RequireWorkspace(workspaceId);
        
        // ensure object exists
        await this.RequireById(workspaceId, id);
        
        // create a protector
        var protector = this.protectionProvider.Create(workspaceId);
        
        // encrypt if needed
        input.Value = input.Encrypted ? protector.Protect(input.Value) : input.Value;
        
        // update in the storage
        return await this.secretRepository.UpdateValueById(workspaceId, id, input);
    }

    /// <summary>
    /// Deletes the object by id
    /// </summary>
    /// <param name="workspaceId">The workspace id</param>
    /// <param name="id">The id of the object</param>
    /// <returns></returns>
    public async Task<SecretModel> DeleteById(string workspaceId, string id)
    {
        // require the parent object
        await this.validationService.RequireWorkspace(workspaceId);
        
        // try deleting object by id
        var result = await this.secretRepository.DeleteById(workspaceId, id);

        // make sure object exists
        if (result == null)
        {
            throw ErrorDefinition.NotFound().AsException();
        }

        // return the object
        return result;
    }
}