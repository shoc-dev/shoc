using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Shoc.ApiCore.GrpcClient;
using Shoc.Core;
using Shoc.Registry.Crypto;
using Shoc.Registry.Data;
using Shoc.Registry.Model;
using Shoc.Registry.Model.Key;

namespace Shoc.Registry.Services;

/// <summary>
/// The registry signing key service
/// </summary>
public class RegistrySigningKeyService : RegistryServiceBase
{
    /// <summary>
    /// The registry credential repository
    /// </summary>
    private readonly IRegistrySigningKeyRepository registrySigningKeyRepository;

    /// <summary>
    /// The key provider service
    /// </summary>
    private readonly KeyProviderService keyProviderService;

    /// <summary>
    /// The protection provider
    /// </summary>
    private readonly SigningKeyProtectionProvider signingKeyProtectionProvider;

    /// <summary>
    /// The registry signing key service
    /// </summary>
    /// <param name="registrySigningKeyRepository">The registry signing key repository</param>
    /// <param name="keyProviderService">The key provider service</param>
    /// <param name="signingKeyProtectionProvider">The protection provider</param>
    /// <param name="registryRepository">The registry repository</param>
    /// <param name="grpcClientProvider">The grpc client provider</param>
    public RegistrySigningKeyService(IRegistrySigningKeyRepository registrySigningKeyRepository, KeyProviderService keyProviderService, SigningKeyProtectionProvider signingKeyProtectionProvider, IRegistryRepository registryRepository, IGrpcClientProvider grpcClientProvider) : base(registryRepository, grpcClientProvider)
    {
        this.registrySigningKeyRepository = registrySigningKeyRepository;
        this.keyProviderService = keyProviderService;
        this.signingKeyProtectionProvider = signingKeyProtectionProvider;
    }

    /// <summary>
    /// Gets all the objects
    /// </summary>
    /// <param name="registryId">The registry id</param>
    /// <returns></returns>
    public async Task<IEnumerable<RegistrySigningKeyModel>> GetAll(string registryId)
    {
        // require parent object to exist
        await this.RequireRegistryById(registryId);
        
        return await this.registrySigningKeyRepository.GetAll(registryId);
    }
    
    /// <summary>
    /// Gets all the key payloads
    /// </summary>
    /// <param name="registryId">The registry id</param>
    /// <returns></returns>
    public async Task<IEnumerable<BaseKeyPayload>> GetAllPayloads(string registryId)
    {
        // load all the items
        var items = await this.GetAll(registryId);
        
        // require the registry
        var registry = await this.RequireRegistryById(registryId);
        
        // create a protector
        var protector = this.signingKeyProtectionProvider.Create(registry.WorkspaceId);

        return items.Select(item =>
        {
            // unprotect the payload JSON
            var plain = protector.Unprotect(item.PayloadEncrypted);

            // deserialize and return
            return this.keyProviderService.Deserialize(item.Algorithm, plain);
        });
    }

    /// <summary>
    /// Gets the object by id
    /// </summary>
    /// <param name="registryId">The registry id</param>
    /// <param name="id">The record id</param>
    /// <returns></returns>
    public async Task<RegistrySigningKeyModel> GetById(string registryId, string id)
    {
        // require parent object to exist
        await this.RequireRegistryById(registryId);

        // id should be a valid string
        if (string.IsNullOrWhiteSpace(id))
        {
            throw ErrorDefinition.NotFound().AsException();
        }
        
        // get the object by id
        var result = await this.registrySigningKeyRepository.GetById(registryId, id);

        // check if result exist
        if (result == null)
        {
            throw ErrorDefinition.NotFound().AsException();
        }

        return result;
    }
    
    /// <summary>
    /// Gets the object by key id
    /// </summary>
    /// <param name="registryId">The registry id</param>
    /// <param name="keyId">The key id</param>
    /// <returns></returns>
    public async Task<RegistrySigningKeyModel> GetByKeyId(string registryId, string keyId)
    {
        // require parent object to exist
        await this.RequireRegistryById(registryId);

        // key should be a valid string
        if (string.IsNullOrWhiteSpace(keyId))
        {
            throw ErrorDefinition.NotFound().AsException();
        }
        
        // get the object by id
        var result = await this.registrySigningKeyRepository.GetByKeyId(registryId, keyId);

        // check if result exist
        if (result == null)
        {
            throw ErrorDefinition.NotFound().AsException();
        }

        return result;
    }

    /// <summary>
    /// Creates the object with given input
    /// </summary>
    /// <param name="registryId">The parent object id</param>
    /// <param name="input">The creation input</param>
    /// <returns></returns>
    public async Task<RegistrySigningKeyModel> Create(string registryId, RegistrySigningKeyCreateModel input)
    {
        // ensure referring the correct object
        input.RegistryId = registryId;
        
        // require the registry
        var registry = await this.RequireRegistryById(input.RegistryId);

        // initialize usage if not given
        input.Usage ??= RegistrySigningKeyUsages.SIGNING;

        // create a key id for the new key
        input.KeyId = Guid.NewGuid().ToString("N").ToUpperInvariant();

        // mark as not X509 certificate
        input.IsX509Certificate = false;
        
        // validate the usage
        ValidateKeyUsage(input.Usage);
        
        // validate the algorithm
        ValidateKeyAlgorithm(input.Algorithm);

        // generate a new key based on the algorithm
        var generated = this.keyProviderService.Generate(input.Algorithm, input.KeyId);

        // serialize the payload
        var payload = this.keyProviderService.Serialize(generated);
        
        // create a protector
        var protector = this.signingKeyProtectionProvider.Create(registry.WorkspaceId);
        
        // encrypt payload and store
        input.PayloadEncrypted = protector.Protect(payload);
        
        // create in the storage
        return await this.registrySigningKeyRepository.Create(input);
    }

    /// <summary>
    /// Deletes the object by id
    /// </summary>
    /// <param name="registryId">The registry id</param>
    /// <param name="id">The id of the object</param>
    /// <returns></returns>
    public async Task<RegistrySigningKeyModel> DeleteById(string registryId, string id)
    {
        // require parent object
        await this.RequireRegistryById(registryId);

        // check if object id is given
        if (string.IsNullOrWhiteSpace(id))
        {
            throw ErrorDefinition.NotFound().AsException();
        }

        // try performing operation and get the result
        var result = await this.registrySigningKeyRepository.DeleteById(registryId, id);

        // check if successful
        if (result == null)
        {
            throw ErrorDefinition.NotFound().AsException();
        }

        return result;
    }
    
    /// <summary>
    /// Validates the given usage
    /// </summary>
    /// <param name="usage">The usage to validate</param>
    private static void ValidateKeyUsage(string usage)
    {
        // ensure usage is defined
        if (!RegistrySigningKeyUsages.ALL.Contains(usage))
        {
            throw ErrorDefinition.Validation(RegistryErrors.INVALID_KEY_USAGE).AsException();
        }
    }
    
    /// <summary>
    /// Validates the given algorithm
    /// </summary>
    /// <param name="algorithm">The algorithm to validate</param>
    private static void ValidateKeyAlgorithm(string algorithm)
    {
        // ensure algorithm is defined
        if (!RegistrySigningKeyAlgorithms.ALL.Contains(algorithm))
        {
            throw ErrorDefinition.Validation(RegistryErrors.INVALID_KEY_ALGORITHM).AsException();
        }
    }
}