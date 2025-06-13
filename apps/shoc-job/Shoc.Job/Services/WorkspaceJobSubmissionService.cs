using System.Threading.Tasks;
using Shoc.Job.Model.Job;
using Shoc.Job.Model.WorkspaceJob;
using Shoc.ObjectAccess.Cluster;
using Shoc.ObjectAccess.Job;
using Shoc.ObjectAccess.Model.Cluster;
using Shoc.ObjectAccess.Model.Package;
using Shoc.ObjectAccess.Model.Workspace;
using Shoc.ObjectAccess.Package;
using Shoc.ObjectAccess.Workspace;

namespace Shoc.Job.Services;

/// <summary>
/// The workspace job submission service
/// </summary>
public class WorkspaceJobSubmissionService : WorkspaceJobServiceBase
{
    /// <summary>
    /// Creates new instance of the service
    /// </summary>
    /// <param name="jobSubmissionService">The job submission service</param>
    /// <param name="jobService">The job service</param>
    /// <param name="workspaceAccessEvaluator">The workspace access evaluator</param>
    /// <param name="packageAccessEvaluator">The package access evaluator</param>
    /// <param name="clusterAccessEvaluator">The cluster access evaluator</param>
    /// <param name="jobAccessEvaluator">The job access evaluator</param>
    public WorkspaceJobSubmissionService(JobSubmissionService jobSubmissionService, JobService jobService, IWorkspaceAccessEvaluator workspaceAccessEvaluator, IPackageAccessEvaluator packageAccessEvaluator, IClusterAccessEvaluator clusterAccessEvaluator, IJobAccessEvaluator jobAccessEvaluator)
        : base(jobSubmissionService, jobService, workspaceAccessEvaluator, packageAccessEvaluator, clusterAccessEvaluator, jobAccessEvaluator)
    {
    }
    
    /// <summary>
    /// Creates a new object
    /// </summary>
    /// <returns></returns>
    public async Task<WorkspaceJobCreatedModel> Create(string userId, string workspaceId, JobSubmissionCreateInput input)
    {
        // ensure have required access to workspace
        await this.workspaceAccessEvaluator.Ensure(
            userId,
            workspaceId,
            WorkspacePermissions.WORKSPACE_VIEW,
            WorkspacePermissions.WORKSPACE_LIST_PACKAGES,
            WorkspacePermissions.WORKSPACE_LIST_CLUSTERS,
            WorkspacePermissions.WORKSPACE_LIST_JOBS,
            WorkspacePermissions.WORKSPACE_SUBMIT_JOB);

        // ensure having the required access to package
        await this.packageAccessEvaluator.Ensure(
            userId,
            workspaceId,
            input.Manifest?.PackageId,
            PackagePermissions.PACKAGE_VIEW,
            PackagePermissions.PACKAGE_USE);
        
        // ensure having the required access to cluster
        await this.clusterAccessEvaluator.Ensure(
            userId,
            workspaceId,
            input.Manifest?.ClusterId,
            ClusterPermissions.CLUSTER_VIEW,
            ClusterPermissions.CLUSTER_USE);
        
        // ensure referring to the correct object
        input.WorkspaceId = workspaceId;
        input.UserId = userId;

        // create the object
        var result = await this.jobSubmissionService.Create(workspaceId, input);
        
        // build result
        return new WorkspaceJobCreatedModel
        {
            Id = result.Id,
            WorkspaceId = result.WorkspaceId,
            LocalId = result.LocalId
        };
    }

    /// <summary>
    /// Submits the job object
    /// </summary>
    /// <returns></returns>
    public async Task<WorkspaceJobCreatedModel> Submit(string userId, string workspaceId, string id, JobSubmissionInput input)
    {
        // ensure referring to the correct object
        input.WorkspaceId = workspaceId;
        input.UserId = userId;
        input.Id = id;
        
        // submit the object
        var result = await this.jobSubmissionService.Submit(workspaceId, id, input);
        
        // build result
        return new WorkspaceJobCreatedModel
        {
            Id = result.Id,
            WorkspaceId = result.WorkspaceId,
            LocalId = result.LocalId
        };
    }
}