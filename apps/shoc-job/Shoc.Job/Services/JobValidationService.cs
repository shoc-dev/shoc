using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shoc.ApiCore.GrpcClient;
using Shoc.Core;
using Shoc.Core.Kubernetes;
using Shoc.Job.Data;
using Shoc.Job.Model;
using Shoc.Job.Model.Job;
using Shoc.Job.Model.JobTask;

namespace Shoc.Job.Services;

/// <summary>
/// The job validation service
/// </summary>
public class JobValidationService : ValidationServiceBase
{
    /// <summary>
    /// The maximum length of the name
    /// </summary>
    protected const int MAX_NAME_LENGTH = 255;
    
    /// <summary>
    /// The maximum length of the description
    /// </summary>
    protected const int MAX_DESCRIPTION_LENGTH = 1024;
    
    /// <summary>
    /// Maximum allowed number of labels
    /// </summary>
    protected const int MAX_LABEL_REFERENCES = 10;

    /// <summary>
    /// The maximum amount of arguments allowed
    /// </summary>
    protected const int MAX_JOB_ARGS = 1000;

    /// <summary>
    /// The maximum length of job array indexer
    /// </summary>
    protected const int MAX_JOB_ARRAY_INDEXER_LENGTH = 1024;

    /// <summary>
    /// The maximum number of replicas in job array
    /// </summary>
    protected const int MAX_JOB_ARRAY_REPLICAS_LIMIT = 10_000;

    /// <summary>
    /// The maximum amount of environment variables
    /// </summary>
    protected const int MAX_JOB_ENVIRONMENT_VARIABLES = 4096;

    /// <summary>
    /// The theoretical maximum of worker replicas
    /// </summary>
    protected const int MAX_MPI_WORKER_REPLICAS = 100_000_000;
    
    /// <summary>
    /// The label repository
    /// </summary>
    protected readonly ILabelRepository labelRepository;

    /// <summary>
    /// The git repo repository
    /// </summary>
    protected readonly IGitRepoRepository gitRepoRepository;

    /// <summary>
    /// The validation service 
    /// </summary>
    /// <param name="grpcClientProvider">The grpc client provider</param>
    /// <param name="labelRepository"></param>
    /// <param name="gitRepoRepository"></param>
    public JobValidationService(IGrpcClientProvider grpcClientProvider, ILabelRepository labelRepository, IGitRepoRepository gitRepoRepository) : base(grpcClientProvider)
    {
        this.labelRepository = labelRepository;
        this.gitRepoRepository = gitRepoRepository;
    }

    /// <summary>
    /// Validate the object name
    /// </summary>
    /// <param name="name">The name to validate</param>
    public virtual void ValidateName(string name)
    {
        // ensure name exists but not longer than allowed
        if (name == null || name.Length > MAX_NAME_LENGTH)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_NAME, "The name is not valid").AsException();
        }
    }
    
    /// <summary>
    /// Validate the object description
    /// </summary>
    /// <param name="description">The description to validate</param>
    public virtual void ValidateDescription(string description)
    {
        // ensure description exists but not longer than allowed
        if (description == null || description.Length > MAX_NAME_LENGTH)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_DESCRIPTION, "The description is not valid").AsException();
        }
    }
    
    /// <summary>
    /// Validate object scope
    /// </summary>
    /// <param name="scope">The scope to validate</param>
    public virtual void ValidateScope(string scope)
    {
        // make sure valid status
        if (JobScopes.ALL.Contains(scope))
        {
            return;
        }

        throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_SCOPE, "The scope is not valid").AsException();
    }

    /// <summary>
    /// Validate given set of label identifiers
    /// </summary>
    /// <param name="workspaceId">The workspace id</param>
    /// <param name="labelIds">The label identifiers</param>
    /// <returns></returns>
    public async Task ValidateLabels(string workspaceId, ICollection<string> labelIds)
    {
        // take only distinct value to compare
        labelIds = labelIds.Distinct().ToList();
        
        // empty list of labels is valid
        if (labelIds.Count == 0)
        {
            return;
        }

        // too many labels
        if (labelIds.Count > MAX_LABEL_REFERENCES)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_LABELS_LIMIT, "Too many labels").AsException();
        }

        // count actual labels by given ids
        var actualCount = await this.labelRepository.CountByIds(workspaceId, labelIds);

        // counts not matching
        if (actualCount != labelIds.Count)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_LABEL_REFERENCE, "Invalid labels referenced").AsException();
        }
    }
    
    /// <summary>
    /// Validate given git repository
    /// </summary>
    /// <param name="workspaceId">The workspace id</param>
    /// <param name="gitRepoId">The git repo reference</param>
    /// <returns></returns>
    public async Task ValidateGitRepo(string workspaceId, string gitRepoId)
    {
        // git repo is not given
        if (gitRepoId == null)
        {
            return;
        }

        // check if empty string is given
        if (gitRepoId.Trim() == string.Empty)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_GIT_REPO, "Empty git repository referenced").AsException();
        }

        // get existing object
        var existing = await this.gitRepoRepository.GetById(workspaceId, gitRepoId);

        // object does not exist
        if (existing == null)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_GIT_REPO, "Invalid reference to git repository").AsException();
        }
    }

    /// <summary>
    /// Validate the input arguments
    /// </summary>
    /// <param name="args">The arguments to accept</param>
    public void ValidateArgs(string[] args)
    {
        // check maximum length of job arguments
        if (args.Length > MAX_JOB_ARGS)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_ARGUMENTS, "Too many arguments").AsException();
        }
    }

    /// <summary>
    /// Validates the configuration of job array replicas
    /// </summary>
    /// <param name="input">The input to validate</param>
    public void ValidateArray(JobRunManifestArrayModel input)
    {
        // check if indexer is empty
        if (string.IsNullOrWhiteSpace(input.Indexer))
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_ARRAY, "Empty indexer for job array").AsException();
        }

        // validate maximum indexer length
        if (input.Indexer.Length > MAX_JOB_ARRAY_INDEXER_LENGTH)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_ARRAY, "Indexer is too long").AsException();
        }
        
        // check non-positive number of replicas
        if (input.Replicas <= 0)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_ARRAY, "Number of replicas should be at least 1").AsException();
        }
        
        // check non-positive number of replicas
        if (input.Replicas > MAX_JOB_ARRAY_REPLICAS_LIMIT)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_ARRAY, "Too many job array replicas").AsException();
        }
    }

    /// <summary>
    /// Validates the environment configuration
    /// </summary>
    /// <param name="input">The input to validate</param>
    public void ValidateEnv(JobRunManifestEnvModel input)
    {
        // check if exceeded amount of secrets
        if (input.Use.Length > MAX_JOB_ENVIRONMENT_VARIABLES)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_ENVIRONMENT, "Too many secrets are used").AsException();
        }
        
        // check if exceeded amount of override values
        if (input.Override.Count > MAX_JOB_ENVIRONMENT_VARIABLES)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_ENVIRONMENT, "Too many environment variable overrides").AsException();
        }
    }

    /// <summary>
    /// Validate the MPI spec if given
    /// </summary>
    /// <param name="mpi">The MPI spec</param>
    public void ValidateMpiSpec(JobRunManifestSpecMpiModel mpi)
    {
        // empty spec is valid
        if (mpi == null)
        {
            return;
        }
        
        // validate the workers spec
        ValidateMpiWorkersSpec(mpi.Workers);
    }

    /// <summary>
    /// Validate the workers spec
    /// </summary>
    /// <param name="workers">The workers spec</param>
    private void ValidateMpiWorkersSpec(JobRunManifestSpecMpiWorkersModel workers)
    {
        // empty workers spec is ok no need to continue
        if (workers == null)
        {
            return;
        }
        
        // number of replicas is specified but not within the range
        if (workers.Replicas is <= 0 or > MAX_MPI_WORKER_REPLICAS)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_SPEC, $"The {workers.Replicas.Value} is not a valid number of workers for MPI").AsException();
        }

        // if distribution strategy is specified but not a valid value
        if (workers.DistributionStrategy != null && !JobTaskMpiDistributionStrategies.ALL.Contains(workers.DistributionStrategy))
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_SPEC, $"The {workers.DistributionStrategy} is not a valid worker distribution strategy for MPI").AsException();
        }
        
        // if distribution unit is specified but not a valid value
        if (workers.DistributionUnit != null && !WellKnownResources.ALL.Contains(workers.DistributionUnit))
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_JOB_SPEC, $"The {workers.DistributionUnit} is not a valid distribution unit for MPI").AsException();
        }
    }
}