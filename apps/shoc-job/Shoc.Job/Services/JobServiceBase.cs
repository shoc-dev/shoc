using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Shoc.Core;
using Shoc.Job.Data;
using Shoc.Job.K8s;
using Shoc.Job.Model.Job;

namespace Shoc.Job.Services;

/// <summary>
/// The base service for job operations
/// </summary>
public abstract class JobServiceBase
{
    /// <summary>
    /// The job repository
    /// </summary>
    protected readonly IJobRepository jobRepository;

    /// <summary>
    /// The job validation service
    /// </summary>
    protected readonly JobValidationService validationService;

    /// <summary>
    /// The protection provider
    /// </summary>
    protected readonly JobProtectionProvider jobProtectionProvider;

    /// <summary>
    /// The job client factory
    /// </summary>
    protected readonly KubernetesJobClientFactory jobClientFactory;
    
    /// <summary>
    /// The task client factory for Kubernetes
    /// </summary>
    protected readonly KubernetesTaskClientFactory taskClientFactory;

    /// <summary>
    /// The task repository
    /// </summary>
    protected readonly IJobTaskRepository taskRepository;

    /// <summary>
    /// The job base service
    /// </summary>
    /// <param name="jobRepository">The job repository</param>
    /// <param name="validationService">The validation service</param>
    /// <param name="jobProtectionProvider">The protection provider</param>
    /// <param name="jobClientFactory">The job client factory</param>
    /// <param name="taskClientFactory">The task client factory</param>
    /// <param name="taskRepository">The task repository</param>
    protected JobServiceBase(IJobRepository jobRepository, JobValidationService validationService, JobProtectionProvider jobProtectionProvider, KubernetesJobClientFactory jobClientFactory, KubernetesTaskClientFactory taskClientFactory, IJobTaskRepository taskRepository)
    {
        this.jobRepository = jobRepository;
        this.validationService = validationService;
        this.jobProtectionProvider = jobProtectionProvider;
        this.jobClientFactory = jobClientFactory;
        this.taskClientFactory = taskClientFactory;
        this.taskRepository = taskRepository;
    }
    
    /// <summary>
    /// Gets the extended object by id
    /// </summary>
    /// <returns></returns>
    protected async Task<JobModel> RequireById(string workspaceId, string id)
    {
        // require the parent object
        await this.validationService.RequireWorkspace(workspaceId);

        // try load the object
        var result = await this.jobRepository.GetById(workspaceId, id);

        // check if object exists
        if (result == null)
        {
            throw ErrorDefinition.NotFound().AsException();
        }
        
        // return result
        return result;
    }
    
    /// <summary>
    /// Converts the object to json string with camel case convention
    /// </summary>
    /// <param name="input">The input to serialize</param>
    /// <typeparam name="T">The type to serialize</typeparam>
    /// <returns></returns>
    protected static string ToJsonString<T>(T input)
    {
        return JsonConvert.SerializeObject(input, new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    ProcessDictionaryKeys = false
                }
            }
        });
    }

    /// <summary>
    /// Deserialize given json into an object
    /// </summary>
    /// <param name="json">The json string</param>
    /// <typeparam name="T">The type to deserialize to</typeparam>
    /// <returns></returns>
    protected static T FromJsonString<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }
}