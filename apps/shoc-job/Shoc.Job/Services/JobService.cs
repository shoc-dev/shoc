using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shoc.Core;
using Shoc.Job.Data;
using Shoc.Job.K8s;
using Shoc.Job.Model.Job;

namespace Shoc.Job.Services;

/// <summary>
/// The job service
/// </summary>
public class JobService : JobServiceBase
{
    /// <summary>
    /// The default page size
    /// </summary>
    private const int DEFAULT_PAGE_SIZE = 20;

    /// <summary>
    /// Creates new instance of the job service
    /// </summary>
    /// <param name="jobRepository">The job repository</param>
    /// <param name="validationService">The validation service</param>
    /// <param name="jobProtectionProvider">The protection provider</param>
    /// <param name="jobClientFactory">The job client factory</param>
    /// <param name="taskClientFactory">The task client factory</param>
    /// <param name="taskRepository">The task repository</param>
    public JobService(IJobRepository jobRepository, JobValidationService validationService, JobProtectionProvider jobProtectionProvider, KubernetesJobClientFactory jobClientFactory, KubernetesTaskClientFactory taskClientFactory, IJobTaskRepository taskRepository) 
        : base(jobRepository, validationService, jobProtectionProvider, jobClientFactory, taskClientFactory, taskRepository)
    {
    }
    
    /// <summary>
    /// Gets all the objects
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<JobModel>> GetAll(string workspaceId)
    {
        // require the parent object
        await this.validationService.RequireWorkspace(workspaceId);

        // get from the storage
        return await this.jobRepository.GetAll(workspaceId);
    }
    
    /// <summary>
    /// Gets page of objects by filter
    /// </summary>
    /// <returns></returns>
    public async Task<JobPageResult<JobExtendedModel>> GetExtendedPageBy(string workspaceId, JobFilter filter, int page, int? size)
    {
        // require the parent object
        await this.validationService.RequireWorkspace(workspaceId);

        // get from the storage
        return await this.jobRepository.GetExtendedPageBy(workspaceId, filter, Math.Abs(page), Math.Abs(size ?? DEFAULT_PAGE_SIZE));
    }
        
    /// <summary>
    /// Gets the object by id
    /// </summary>
    /// <returns></returns>
    public Task<JobModel> GetById(string workspaceId, string id)
    {
        return this.RequireById(workspaceId, id);
    }
    
    /// <summary>
    /// Gets the extended object by id
    /// </summary>
    /// <returns></returns>
    public async Task<JobExtendedModel> GetExtendedById(string workspaceId, string id)
    {
        // require the parent object
        await this.validationService.RequireWorkspace(workspaceId);

        // try load the object
        var result = await this.jobRepository.GetExtendedById(workspaceId, id);

        // check if object exists
        if (result == null)
        {
            throw ErrorDefinition.NotFound().AsException();
        }
        
        // return result
        return result;
    }
    
    /// <summary>
    /// Gets the extended object by local id
    /// </summary>
    /// <returns></returns>
    public async Task<JobExtendedModel> GetExtendedByLocalId(string workspaceId, long localId)
    {
        // require the parent object
        await this.validationService.RequireWorkspace(workspaceId);

        // try load the object
        var result = await this.jobRepository.GetExtendedByLocalId(workspaceId, localId);

        // check if object exists
        if (result == null)
        {
            throw ErrorDefinition.NotFound().AsException();
        }
        
        // return result
        return result;
    }
}