using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Serialization;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;
using Shoc.Core;
using Shoc.Package.Data;
using Shoc.Package.Model;
using Shoc.Package.Model.BuildTask;
using Shoc.Package.Model.Command;
using Shoc.Package.Model.Package;
using Shoc.Package.Model.Registry;
using Shoc.Package.Templating.Model;

namespace Shoc.Package.Services;

/// <summary>
/// The build task service
/// </summary>
public class BuildTaskService
{
    /// <summary>
    /// The name of the default variant
    /// </summary>
    private const string DEFAULT_TEMPLATE_VARIANT = "default";

    /// <summary>
    /// The zip bundle name
    /// </summary>
    private const string SHOC_BUNDLE_ZIP_NAME = "bundle.zip";
    
    /// <summary>
    /// The special generated Containerfile for the build
    /// </summary>
    private const string SHOC_CONTAINERFILE_NAME = "Containerfile.g.shoc";
    
    /// <summary>
    /// The prefix for temporary build root directory
    /// </summary>
    private const string SHOC_BUILD_ROOT_PREFIX = "shoc_build_";

    /// <summary>
    /// The name of build subdirectory
    /// </summary>
    private const string BUILD_DIRECTORY_NAME = "build";

    /// <summary>
    /// The prefix for default values
    /// </summary>
    private const string TEMPLATE_DEFAULTS_PREFIX = "system";
    
    /// <summary>
    /// The deadline of object creation
    /// </summary>
    private static readonly TimeSpan CREATING_DEADLINE = TimeSpan.FromMinutes(5);
    
    /// <summary>
    /// The build task repository
    /// </summary>
    private readonly IBuildTaskRepository buildTaskRepository;

    /// <summary>
    /// The package repository
    /// </summary>
    private readonly IPackageRepository packageRepository;

    /// <summary>
    /// The template provider
    /// </summary>
    private readonly TemplateProvider templateProvider;

    /// <summary>
    /// The validation service
    /// </summary>
    private readonly ValidationService validationService;

    /// <summary>
    /// The container service
    /// </summary>
    private readonly ContainerService containerService;

    /// <summary>
    /// The registry handler service
    /// </summary>
    private readonly RegistryHandlerService registryHandlerService;

    /// <summary>
    /// Creates new instance of the service
    /// </summary>
    /// <param name="buildTaskRepository">The build task repository</param>
    /// <param name="packageRepository">The package repository</param>
    /// <param name="templateProvider">The template provider</param>
    /// <param name="validationService">The validation service</param>
    /// <param name="containerService">The container service</param>
    /// <param name="registryHandlerService">The registry handler service</param>
    public BuildTaskService(IBuildTaskRepository buildTaskRepository, IPackageRepository packageRepository, TemplateProvider templateProvider, ValidationService validationService, ContainerService containerService, RegistryHandlerService registryHandlerService)
    {
        this.buildTaskRepository = buildTaskRepository;
        this.packageRepository = packageRepository;
        this.templateProvider = templateProvider;
        this.validationService = validationService;
        this.containerService = containerService;
        this.registryHandlerService = registryHandlerService;
    }
    
    /// <summary>
    /// Gets all the objects
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<BuildTaskModel>> GetAll(string workspaceId)
    {
        // require the parent object
        await this.validationService.RequireWorkspace(workspaceId);

        // get from the storage
        return await this.buildTaskRepository.GetAll(workspaceId);
    }
    
    /// <summary>
    /// Gets the object by id
    /// </summary>
    /// <returns></returns>
    public async Task<BuildTaskModel> GetById(string workspaceId, string id)
    {
        // require the parent object
        await this.validationService.RequireWorkspace(workspaceId);

        // get from the storage
        var result = await this.buildTaskRepository.GetById(workspaceId, id);

        // ensure object exists
        if (result == null)
        {
            throw ErrorDefinition.NotFound().AsException();
        }

        return result;
    }
    
    /// <summary>
    /// Creates the object with given input
    /// </summary>
    /// <param name="workspaceId">The workspace id</param>
    /// <param name="input">The creation input</param>
    /// <returns></returns>
    public async Task<BuildTaskModel> Create(string workspaceId, BuildTaskCreateModel input)
    {
        // ensure referring to the correct object
        input.WorkspaceId = workspaceId;

        // initial status is creating
        input.Status = BuildTaskStatuses.CREATED;
        
        // initialize deadline
        input.Deadline = DateTime.UtcNow.Add(CREATING_DEADLINE);
        
        // require the parent object
        await this.validationService.RequireWorkspace(workspaceId);

        // require valid user
        await this.validationService.RequireUser(input.UserId);
        
        // validate provider
        ValidateProvider(input.Provider);
        
        // validate target scope
        this.validationService.ValidateScope(input.Scope);
        
        // validate the checksum
        this.validationService.ValidateListingChecksum(input.ListingChecksum);

        // deserialize the manifest
        var manifest = DeserializeManifest(input.Manifest);
        
        // gets the template reference
        var templateReference = GetTemplateReference(manifest.Template);
        
        // assign the template reference
        input.TemplateReference = $"{templateReference.Template}:{templateReference.Variant}";

        // get the template variant
        var templateVariant = await this.GetVariant(templateReference.Template, templateReference.Variant);

        // get the variant spec
        var buildSpec = await this.GetVariantBuildSpec(templateReference.Template, templateReference.Variant);
        
        // validate the spec against the schema
        ValidateBuildSpec(buildSpec, manifest.Spec);

        // get the spec defaults based on template declared runtime type
        var specDefaults = GetSpecDefaults(templateVariant.Runtime.Type);
        
        // build the final runtime
        var runtime = BuildRuntime(templateVariant.Runtime, manifest.Spec, specDefaults);
        
        // serialize and store the runtime (ensure camel case)
        input.Runtime = JsonConvert.SerializeObject(runtime, new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    ProcessDictionaryKeys = false
                }
            }
        });
        
        // render containerfile based on the template and specification
        input.Containerfile = await RenderContainerfile(templateVariant.Containerfile, manifest.Spec, specDefaults);
        
        Console.WriteLine(input.Containerfile);

        // gets the registry to store the image
        input.RegistryId = (await this.registryHandlerService.GetDefaultRegistryId(workspaceId)).Id;
        
        // create object in the storage
        return await this.buildTaskRepository.Create(workspaceId, input);
    }

    /// <summary>
    /// Updates the bundle by id
    /// </summary>
    /// <param name="userId">The acting user id</param>
    /// <param name="workspaceId">The workspace id</param>
    /// <param name="id">The record id</param>
    /// <param name="file">The file</param>
    /// <returns></returns>
    public async Task<BuildTaskModel> UpdateBundleById(string userId, string workspaceId, string id, Stream file)
    {
        // get and require existing object
        var existing = await this.GetById(workspaceId, id);

        // the user performing the operation is different
        if (existing.UserId != userId)
        {
            throw ErrorDefinition.Validation(PackageErrors.USER_MISMATCH).AsException();
        }
        
        // create a root directory for the build
        var rootDir = Directory.CreateTempSubdirectory(SHOC_BUILD_ROOT_PREFIX);

        try
        {
            // perform the action
            return await this.UpdateBundleByIdImpl(existing, rootDir, file);
        }
        catch (ShocException e)
        {
            return await this.buildTaskRepository.UpdateById(existing.WorkspaceId, existing.Id, new BuildTaskUpdateModel
            {
                WorkspaceId = existing.WorkspaceId,
                Id = existing.Id,
                Status = BuildTaskStatuses.FAILED,
                Deadline = null,
                ErrorCode = e.Errors.FirstOrDefault()?.Code ?? PackageErrors.UNKNOWN_ERROR,
                Message = e.Errors.FirstOrDefault()?.Message ?? e.Message,
                PackageId = null
            });
        }
        catch (Exception e)
        {
            return await this.buildTaskRepository.UpdateById(existing.WorkspaceId, existing.Id, new BuildTaskUpdateModel
            {
                WorkspaceId = existing.WorkspaceId,
                Id = existing.Id,
                Status = BuildTaskStatuses.FAILED,
                Deadline = null,
                ErrorCode = PackageErrors.UNKNOWN_ERROR,
                Message = e.Message,
                PackageId = null
            });
        }
        finally
        {
            rootDir.Delete(true);
        }
    }

    /// <summary>
    /// Updates the bundle by id
    /// </summary>
    /// <param name="existing">The existing object</param>
    /// <param name="rootDir">The root directory</param>
    /// <param name="file">The file</param>
    /// <returns></returns>
    private async Task<BuildTaskModel> UpdateBundleByIdImpl(BuildTaskModel existing, DirectoryInfo rootDir, Stream file)
    {
        // check if status is not in a creating status then error
        if (existing.Status != BuildTaskStatuses.CREATED)
        {
            throw ErrorDefinition.Validation(PackageErrors.UNEXPECTED_OPERATION).AsException();
        }

        // the build has expired in creation status
        if (existing.Deadline < DateTime.UtcNow)
        {
            throw ErrorDefinition.Validation(PackageErrors.EXPIRED_BUILD_TASK).AsException();
        }

        // check the provider
        if (existing.Provider != BuildTaskProviders.REMOTE)
        {
            throw ErrorDefinition.Validation(PackageErrors.INVALID_BUILD_PROVIDER).AsException();
        }

        // empty file for zip
        var zipFile = Path.Combine(rootDir.FullName, SHOC_BUNDLE_ZIP_NAME);
        
        try
        {
            await using var zipStream = new FileStream(zipFile, FileMode.OpenOrCreate, FileAccess.Write);
            await file.CopyToAsync(zipStream);
        }
        catch (Exception ex)
        {
            // rethrow with proper exception
            throw ErrorDefinition.Validation(PackageErrors.UPLOAD_ERROR, ex.Message).AsException();
        }

        // create a subdirectory
        var buildDir = rootDir.CreateSubdirectory(BUILD_DIRECTORY_NAME);

        try
        {
            ZipFile.ExtractToDirectory(zipFile, buildDir.FullName);
        }
        catch (Exception ex)
        {
            // rethrow with proper exception
            throw ErrorDefinition.Validation(PackageErrors.UNZIP_ERROR, ex.Message).AsException();
        }
        
        // write containerfile
        await File.WriteAllTextAsync(Path.Combine(rootDir.FullName, SHOC_CONTAINERFILE_NAME), existing.Containerfile);

        // get registry
        var registry = await this.registryHandlerService.GetRegistryById(existing.RegistryId);

        // build the future package id
        var packageId = StdIdGenerator.Next(PackageObjects.PACKAGE).ToLowerInvariant();

        // build image url
        var image = this.registryHandlerService.BuildImageTag(new RegistryImageContext
        {
            Registry = registry.Registry,
            Namespace = registry.Namespace,
            Provider = registry.Provider,
            TargetWorkspaceId = existing.WorkspaceId,
            TargetUserId = existing.UserId,
            TargetPackageScope = existing.Scope,
            TargetPackageId = packageId
        });
        
        // create a build context
        var buildContext = new ContainerBuildContext
        {
            Containerfile = Path.Combine(rootDir.FullName, SHOC_CONTAINERFILE_NAME),
            WorkingDirectory = buildDir.FullName,
            Image = image
        };
        
        // the build result
        var buildResult = await this.containerService.Build(buildContext);
        
        // build was not successful
        if (!buildResult.Success)
        {
            throw ErrorDefinition.Validation(PackageErrors.IMAGE_BUILD_ERROR, buildResult.Error).AsException();
        }

        // get the push credential
        var credential = await this.registryHandlerService.GetPushCredential(existing.RegistryId, existing.WorkspaceId, existing.UserId);
        
        // create push context
        var pushContext = new ContainerPushContext
        {
            Image = image,
            Username = credential.Username,
            Password = credential.PasswordPlain
        };

        // push and get the result
        var pushResult = await this.containerService.Push(pushContext);
        
        // remove the local image after successful push
        await this.containerService.RemoveImage(new ContainerRmiContext { Image = image});
        
        // push was not successful
        if (!pushResult.Success)
        {
            throw ErrorDefinition.Validation(PackageErrors.IMAGE_PUSH_ERROR, pushResult.Error).AsException();
        }

        // store the package
        var package = await this.packageRepository.Create(existing.WorkspaceId, new PackageCreateModel
        {
            Id = packageId,
            WorkspaceId = existing.WorkspaceId,
            UserId = existing.UserId,
            Scope = existing.Scope,
            ListingChecksum = existing.ListingChecksum,
            Manifest = existing.Manifest,
            Runtime = existing.Runtime,
            Containerfile = existing.Containerfile,
            TemplateReference = existing.TemplateReference,
            RegistryId = existing.RegistryId,
            Image = image
        });
        
        // update the status
        return await this.buildTaskRepository.UpdateById(existing.WorkspaceId, existing.Id, new BuildTaskUpdateModel
        {
            WorkspaceId = existing.WorkspaceId,
            Id = existing.Id,
            Status = BuildTaskStatuses.COMPLETED,
            Deadline = null,
            ErrorCode = null,
            Message = $"Package {image} is successfully created",
            PackageId = package.Id
        });
    }

    /// <summary>
    /// Renders the given template with the given spec
    /// </summary>
    /// <param name="containerfileTemplate">The template content</param>
    /// <param name="spec">The specification to render</param>
    /// <param name="defaults">The default values</param>
    /// <returns></returns>
    private static async Task<string> RenderContainerfile(string containerfileTemplate, Dictionary<string, object> spec, PackageSpecDefaultsModel defaults)
    {
        // the defaults map
        var defaultsMap = new Dictionary<string, object> { { TEMPLATE_DEFAULTS_PREFIX, defaults } };
        
        try
        {
            // parse the template file
            var parsed = Template.Parse(containerfileTemplate);

            // create new context based on the lexer lang option
            var context = parsed.LexerOptions.Lang == ScriptLang.Liquid ? new LiquidTemplateContext() : new TemplateContext();

            // new script object
            var scriptObject = new ScriptObject();

            // import spec
            scriptObject.Import(spec);
            
            // import defaults map
            scriptObject.Import(defaultsMap);
            
            // add script object to the context
            context.PushGlobal(scriptObject);
            
            // render with context
            return await parsed.RenderAsync(context);
        }
        catch (Exception e)
        {
            throw ErrorDefinition.Validation(PackageErrors.RENDER_FAILURE, e.Message).AsException();
        }
    }
    
    /// <summary>
    /// Gets the template variant
    /// </summary>
    /// <param name="name">The name of the template</param>
    /// <param name="variant">The template variant</param>
    /// <returns></returns>
    protected async Task<TemplateVariantDefinition> GetVariant(string name, string variant)
    {
        try
        {
            return await this.templateProvider.GetVariant(name, variant);
        }
        catch (Exception)
        {
            throw ErrorDefinition.Validation(PackageErrors.INVALID_TEMPLATE).AsException();
        }
    }
    
    /// <summary>
    /// Gets the build spec schema
    /// </summary>
    /// <param name="name">The name of the template</param>
    /// <param name="variant">The template variant</param>
    /// <returns></returns>
    protected async Task<JSchema> GetVariantBuildSpec(string name, string variant)
    {
        try
        {
            return await this.templateProvider.GetVariantBuildSpec(name, variant);
        }
        catch (Exception)
        {
            throw ErrorDefinition.Validation(PackageErrors.INVALID_BUILD_SPEC).AsException();
        }
    }

    /// <summary>
    /// Validates the build spec object against the schema
    /// </summary>
    /// <param name="schema">The schema</param>
    /// <param name="spec">The spec</param>
    protected void ValidateBuildSpec(JSchema schema, Dictionary<string, object> spec)
    {
        // no spec was provided
        if (spec == null)
        {
            throw ErrorDefinition.Validation(PackageErrors.INVALID_BUILD_SPEC).AsException();
        }
        
        // parse the object
        var specObject = JObject.FromObject(spec);
        
        // check if not valid
        if (!specObject.IsValid(schema, out IList<ValidationError> errors))
        {
            throw ErrorDefinition.Validation(PackageErrors.INVALID_BUILD_SPEC, errors.FirstOrDefault()?.Message).AsException();
        }
    }
    
    /// <summary>
    /// Validate object provider
    /// </summary>
    /// <param name="provider">The provider to validate</param>
    protected static void ValidateProvider(string provider)
    {
        // make sure valid status
        if (BuildTaskProviders.ALL.Contains(provider))
        {
            return;
        }

        throw ErrorDefinition.Validation(PackageErrors.INVALID_BUILD_PROVIDER).AsException();
    }

    /// <summary>
    /// Deserialize the manifest json to a proper format
    /// </summary>
    /// <param name="manifest">The manifest string</param>
    /// <returns></returns>
    protected static BuildManifestModel DeserializeManifest(string manifest)
    {
        // do not allow empty manifest
        if (string.IsNullOrWhiteSpace(manifest))
        {
            throw ErrorDefinition.Validation(PackageErrors.INVALID_MANIFEST).AsException();
        }

        try
        {
            // using Newtonsoft to make sure Dictionary<string, object> has simple values not Json Elements
            return JsonConvert.DeserializeObject<BuildManifestModel>(manifest);
        }
        catch (Exception e)
        {
            throw ErrorDefinition.Validation(PackageErrors.INVALID_MANIFEST, e.Message).AsException();
        }
    }

    /// <summary>
    /// Gets the template reference
    /// </summary>
    /// <param name="template">The template string</param>
    /// <returns></returns>
    protected static TemplateReference GetTemplateReference(string template)
    {
        // ensure no empty template
        if (string.IsNullOrWhiteSpace(template))
        {
            throw ErrorDefinition.Validation(PackageErrors.INVALID_TEMPLATE).AsException();
        }

        // split template by parts
        var parts = template.Split(':', 2, StringSplitOptions.RemoveEmptyEntries);

        // the name of the template
        var name = parts[0];

        // the variant
        var variant = DEFAULT_TEMPLATE_VARIANT;

        // check if there is a second part and it's not empty
        if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]))
        {
            variant = parts[1];
        }

        return new TemplateReference
        {
            Template = name,
            Variant = variant
        };
    }

    /// <summary>
    /// Builds the runtime object based on the static runtime and spec fields
    /// </summary>
    /// <param name="runtime">The static runtime</param>
    /// <param name="spec">The specification data</param>
    /// <param name="defaults">The default values</param>
    /// <returns></returns>
    private static TemplateRuntimeModel BuildRuntime(TemplateRuntimeModel runtime, IReadOnlyDictionary<string, object> spec, PackageSpecDefaultsModel defaults)
    {
        // try getting uid from spec, otherwise fallback to runtime given value or default
        var uid = spec.TryGetValue(KnownRuntimeProperties.UID, out var givenUid) ? (long) givenUid : runtime.Uid ?? defaults.Uid;

        // try getting user from spec, otherwise fallback to runtime given value or default
        var user = spec.GetValueOrDefault(KnownRuntimeProperties.USER, null) as string ?? runtime.User ?? defaults.User;

        // try getting entrypoint from spec, otherwise fallback to runtime given value or default
        var entrypointParsed = (spec.GetValueOrDefault(KnownRuntimeProperties.ENTRYPOINT, null) as JArray)?.ToObject<string[]>();
        var entrypoint = entrypointParsed ?? runtime.Entrypoint ?? [];
        
        // try getting implementation from spec, otherwise fallback to runtime given value or default
        var implementation = spec.GetValueOrDefault(KnownRuntimeProperties.IMPLEMENTATION, null) as string ?? runtime.User ?? defaults.Implementation;

        // build runtime model
        return new TemplateRuntimeModel
        {
            Type = runtime.Type,
            Args = runtime.Args,
            Uid = uid,
            User = user,
            Entrypoint = entrypoint,
            Implementation = implementation
        };
    }

    /// <summary>
    /// The spec defaults
    /// </summary>
    /// <param name="type">The type of the runtime</param>
    /// <returns></returns>
    private static PackageSpecDefaultsModel GetSpecDefaults(string type)
    {
        // the implementation value
        var implementation = type switch
        {
            RuntimeTypes.FUNCTION => PackageDefaultValues.FUNCTION_IMPLEMENTATION,
            RuntimeTypes.MPI => PackageDefaultValues.MPI_IMPLEMENTATION,
            _ => string.Empty,
        };
        
        return new PackageSpecDefaultsModel
        {
            Uid = PackageDefaultValues.UID,
            User = PackageDefaultValues.USER,
            Implementation = implementation
        };
    }
}