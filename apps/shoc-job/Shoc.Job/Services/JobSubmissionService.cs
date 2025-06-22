using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using k8s.Models;
using Microsoft.AspNetCore.DataProtection;
using Quartz;
using Shoc.Core;
using Shoc.Core.Kubernetes;
using Shoc.Job.Allocator;
using Shoc.Job.Data;
using Shoc.Job.K8s;
using Shoc.Job.K8s.Model;
using Shoc.Job.K8s.TaskClients;
using Shoc.Job.Model;
using Shoc.Job.Model.Cluster;
using Shoc.Job.Model.Job;
using Shoc.Job.Model.JobGitRepo;
using Shoc.Job.Model.JobLabel;
using Shoc.Job.Model.JobTask;
using Shoc.Job.Quartz;
using Shoc.Secret.Grpc.Secrets;
using Shoc.Workspace.Grpc.Workspaces;

namespace Shoc.Job.Services;

/// <summary>
/// The job submission service
/// </summary>
public class JobSubmissionService : JobServiceBase
{
    /// <summary>
    /// The default indexer for job array
    /// </summary>
    protected const string DEFAULT_JOB_ARRAY_INDEXER = "SHOC_JOB_ARRAY_INDEXER";

    /// <summary>
    /// The default job array count
    /// </summary>
    protected const string DEFAULT_JOB_ARRAY_COUNT = "SHOC_JOB_ARRAY_COUNT";

    /// <summary>
    /// The default number of replicas in job array
    /// </summary>
    protected const int DEFAULT_JOB_ARRAY_REPLICAS = 1;

    /// <summary>
    /// The package resolver
    /// </summary>
    protected readonly JobPackageResolver packageResolver;

    /// <summary>
    /// The cluster resolver
    /// </summary>
    protected readonly JobClusterResolver clusterResolver;

    /// <summary>
    /// The secret resolver
    /// </summary>
    protected readonly JobSecretResolver secretResolver;

    /// <summary>
    /// The resource parser
    /// </summary>
    protected readonly ResourceParser resourceParser;

    /// <summary>
    /// The resource formatter
    /// </summary>
    protected readonly JobResourceFormatter resourceFormatter;
    
    /// <summary>
    /// The task status repository
    /// </summary>
    protected readonly IJobTaskStatusRepository taskStatusRepository;

    /// <summary>
    /// The scheduler factory
    /// </summary>
    protected readonly ISchedulerFactory schedulerFactory;

    /// <summary>
    /// The allocator factory
    /// </summary>
    protected readonly JobResourceAllocatorFactory allocatorFactory;

    /// <summary>
    /// Creates new instance of job submission service
    /// </summary>
    /// <param name="jobRepository">The job repository</param>
    /// <param name="validationService">The validation service</param>
    /// <param name="jobProtectionProvider">The job protection provider</param>
    /// <param name="taskRepository">The task repository</param>
    /// <param name="packageResolver">The package resolver</param>
    /// <param name="clusterResolver">The cluster resolver</param>
    /// <param name="secretResolver">The secret resolver</param>
    /// <param name="resourceParser">The resource parser</param>
    /// <param name="resourceFormatter">The resource formatter</param>
    /// <param name="taskClientFactory">The task client factory for Kubernetes</param>
    /// <param name="taskStatusRepository">The task status repository</param>
    /// <param name="schedulerFactory">The scheduler factory</param>
    /// <param name="allocatorFactory">The allocator factory</param>
    public JobSubmissionService(IJobRepository jobRepository, JobValidationService validationService, JobProtectionProvider jobProtectionProvider, IJobTaskRepository taskRepository, JobPackageResolver packageResolver, JobClusterResolver clusterResolver, JobSecretResolver secretResolver, ResourceParser resourceParser, JobResourceFormatter resourceFormatter, KubernetesTaskClientFactory taskClientFactory, IJobTaskStatusRepository taskStatusRepository, ISchedulerFactory schedulerFactory, JobResourceAllocatorFactory allocatorFactory) 
        : base(jobRepository, validationService, jobProtectionProvider, taskClientFactory, taskRepository)
    {
        this.packageResolver = packageResolver;
        this.clusterResolver = clusterResolver;
        this.secretResolver = secretResolver;
        this.resourceParser = resourceParser;
        this.resourceFormatter = resourceFormatter;
        this.taskStatusRepository = taskStatusRepository;
        this.schedulerFactory = schedulerFactory;
        this.allocatorFactory = allocatorFactory;
    }
    
    /// <summary>
    /// Gets the object by id
    /// </summary>
    /// <returns></returns>
    public async Task<JobModel> GetById(string workspaceId, string id)
    {
        // require the parent object
        await this.validationService.RequireWorkspace(workspaceId);

        // get from the storage
        var result = await this.jobRepository.GetById(workspaceId, id);

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
    public async Task<JobModel> Create(string workspaceId, JobSubmissionCreateInput input)
    {
        // enrich with defaults
        input = BuildInputWithDefaults(workspaceId, input);

        // validate the given input
        await this.ValidateInput(input);

        // the protection provider
        var protector = this.jobProtectionProvider.Create();
        
        // build wrapped arguments model
        var args = new JobTaskArgsModel
        {
            Args = input.Manifest.Args
        };
        
        // arguments in json
        var argsJson = ToJsonString(args);
        
        // resolve the referenced package
        var package = await this.packageResolver.ResolveById(input.WorkspaceId, input.Manifest.PackageId);
        
        // deserialize the runtime model
        var runtime = DeserializeRuntime(package.Runtime);

        // the runtime as json back serialized
        var runtimeJson = ToJsonString(runtime);

        // map the runtime
        var taskType = MapTaskType(runtime.Type);
        
        // gets the user's pull credentials for the package's registry within the workspace
        var credentials = await this.packageResolver.GetPullCredential(package.RegistryId, input.WorkspaceId, input.UserId);
        
        // build package reference
        var packageReference = new JobTaskPackageReferenceModel
        {
            Image = package.Image,
            PullUsername = credentials.Username,
            PullPasswordPlain = credentials.PasswordPlain
        };
        
        // the package reference json
        var packageReferenceJson = ToJsonString(packageReference);
        var packageReferenceJsonProtected = protector.Protect(packageReferenceJson);
        
        // resolve the cluster with proper validation
        var cluster = await this.clusterResolver.ResolveById(input.WorkspaceId, input.Manifest.ClusterId);

        // get all node resources
        var nodeResources = await this.GetClusterNodes(cluster.Configuration);

        // create the allocator
        var allocator = this.allocatorFactory.Create(taskType, nodeResources);
            
        // combined set of decrypted environment key values
        var env = await this.ResolveEnvironment(input);
        
        // the resolved env json
        var resolvedEnvJson = ToJsonString(env);
        var resolvedEnvJsonProtected = protector.Protect(resolvedEnvJson);

        // check if cluster configuration is missing
        if (string.IsNullOrWhiteSpace(cluster.Configuration))
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_CLUSTER, "The cluster configuration is missing").AsException();
        }

        // enrich and get new manifest with allocated resources
        var manifest = allocator.Allocate(input.Manifest);

        // parse the resources from manifest
        var resources = manifest.Resources;
        
        // create a job instance
        var job = new JobModel
        {
            WorkspaceId = input.WorkspaceId,
            LocalId = 0,
            ClusterId = cluster.Id,
            UserId = input.UserId,
            Name = input.Manifest.Name,
            Description = input.Manifest.Description,
            Scope = input.Scope,
            Manifest = ToJsonString(input.Manifest),
            ClusterConfigEncrypted = protector.Protect(cluster.Configuration),
            TotalTasks = input.Manifest.Array.Replicas.GetValueOrDefault(DEFAULT_JOB_ARRAY_REPLICAS),
            SucceededTasks = 0,
            FailedTasks = 0,
            CancelledTasks = 0,
            CompletedTasks = 0,
            Status = JobStatuses.CREATED,
            Message = string.Empty,
            PendingAt = null,
            RunningAt = null,
            CompletedAt = null,
            CleanupAt = null
        };

        // a set of tasks
        var tasks = new List<JobTaskModel>();
        
        // the spec as json
        var specJson = ToJsonString(manifest.Spec);
        
        // create tasks corresponding to number of replicas
        for (var i = 0; i < job.TotalTasks; ++i)
        {
            tasks.Add(new JobTaskModel
            {
                WorkspaceId = input.WorkspaceId,
                ClusterId = input.Manifest.ClusterId,
                PackageId = input.Manifest.PackageId,
                UserId = input.UserId,
                Type = taskType,
                Runtime = runtimeJson,
                Args = argsJson,
                PackageReferenceEncrypted = packageReferenceJsonProtected,
                ArrayReplicas = job.TotalTasks,
                ArrayIndexer = input.Manifest.Array.Indexer,
                ArrayCounter = input.Manifest.Array.Counter,
                ResolvedEnvEncrypted = resolvedEnvJsonProtected,
                MemoryRequested = resources?.Memory,
                CpuRequested = resources?.Cpu,
                NvidiaGpuRequested = resources?.NvidiaGpu,
                AmdGpuRequested = resources?.AmdGpu,
                Spec = specJson,
                Status = JobTaskStatuses.CREATED,
                Message = string.Empty,
                PendingAt = null,
                RunningAt = null,
                CompletedAt = null
            });
        }

        // build job and label associations for every job 
        var labels = input.Manifest
            .LabelIds
            .Select(labelId => new JobLabelModel { WorkspaceId = input.WorkspaceId, LabelId = labelId })
            .ToList();

        // git repo association instance if given
        var gitRepo = string.IsNullOrWhiteSpace(input.Manifest.GitRepoId) ? null : new JobGitRepoModel
        {
            WorkspaceId = input.WorkspaceId,
            GitRepoId = input.Manifest.GitRepoId
        };

        // returns the created object
        return await this.jobRepository.Create(input.WorkspaceId, new JobCreateModel
        {
            Job = job,
            Tasks = tasks,
            Labels = labels,
            GitRepo = gitRepo
        });
    }

    /// <summary>
    /// Submit the created job to the target cluster
    /// </summary>
    /// <param name="workspaceId">The workspace id</param>
    /// <param name="id">The job id</param>
    /// <param name="input">The input object</param>
    /// <returns></returns>
    public async Task<JobModel> Submit(string workspaceId, string id, JobSubmissionInput input)
    {
        // load the workspace
        var workspace = await this.validationService.RequireWorkspace(workspaceId);
        
        // get the job instance
        var job = await this.GetById(workspaceId, id);

        // the user initiating the job is not matching the submitting user
        if (!string.Equals(job.UserId, input.UserId))
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_USER, "The submitting user does not match the invoking user").AsException();
        }

        // ensure only job is just created
        if (job.Status != JobStatuses.CREATED)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_STATUS, "The job was already submitted").AsException();
        }

        try
        {
            // perform submission
            return await this.SubmitImpl(workspace, job);
        }
        catch (Exception e)
        {
            // mark job as failed
            return await this.jobRepository.FailById(workspaceId, id, new JobFailInputModel
            {
                WorkspaceId = workspaceId,
                Id = id,
                Message = $"Job failed: {e.Message}",
                CompletedAt = DateTime.UtcNow
            });
        }
        
    }

    /// <summary>
    /// The implementation of job submission logic
    /// </summary>
    /// <param name="workspace">The workspace reference</param>
    /// <param name="job">The job model reference</param>
    /// <returns></returns>
    private async Task<JobModel> SubmitImpl(WorkspaceGrpcModel workspace, JobModel job)
    {
        // load the tasks associated with the job
        var tasks = (await this.taskRepository.GetAll(workspace.Id, job.Id)).OrderBy(task => task.Sequence).ToList();

        // check if number of existing tasks matches the jobs total tasks
        if (tasks.Count != job.TotalTasks)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_TASKS, "The job tasks are not valid").AsException();
        }

        // keep only tasks that are in created state
        tasks = tasks.Where(task => task.Status == JobTaskStatuses.CREATED).ToList();

        // no tasks to process
        if (tasks.Count == 0)
        {
            return job;
        }
        
        // the protection provider
        var protector = this.jobProtectionProvider.Create();

        // the cluster configuration
        var clusterConfig = protector.Unprotect(job.ClusterConfigEncrypted);
        
        // we assume that all the tasks in the job has same package reference
        var packageReference = FromJsonString<JobTaskPackageReferenceModel>(protector.Unprotect(tasks[0].PackageReferenceEncrypted));
        
        // we assume that all the tasks in the job has same package reference
        var envs = FromJsonString<JobTaskEnvModel>(protector.Unprotect(tasks[0].ResolvedEnvEncrypted));

        // create a task submission client for the cluster, assuming all the tasks has same type
        using var taskClient = this.taskClientFactory.Create(clusterConfig, tasks[0].Type);

        // check if the task can be executed on the given cluster
        await taskClient.EnsureSupported();

        // the job client for kubernetes
        using var jobClient = new KubernetesJobClient(clusterConfig);

        // build the namespace name from workspace name and job local id
        var ns = $"job-{workspace.Name}-{job.LocalId}";
        
        // update the namespace of the job
        job = await this.jobRepository.UpdateNamespaceById(workspace.Id, job.Id, ns);

        // create a namespace object in the cluster
        var nsResult = await jobClient.InitNamespace(job);

        // initialize the service account within the namespace
        var saResult = await jobClient.WithCleanup(job.Namespace, () => jobClient.InitServiceAccount(job));

        // initialize the shared secrets for all the job tasks
        var envsResult = await jobClient.WithCleanup(job.Namespace, () => jobClient.InitSharedEnvironment(job, envs));
        
        // initialize the shared pull secret for the package
        var pullSecretResult = await jobClient.WithCleanup(job.Namespace, () => jobClient.InitPullSecret(job, packageReference));
        
        // initiate all submissions for all tasks
        var allSubmissions = tasks.Select(task => this.SubmitTask(taskClient, new InitTaskInput
        {
            Job = job,
            Task = task,
            Spec = FromJsonString<JobRunManifestSpecModel>(task.Spec),
            Runtime = DeserializeRuntime(task.Runtime),
            Namespace = nsResult.Namespace.Name(),
            ServiceAccount = saResult.ServiceAccount.Name(),
            PullSecret = pullSecretResult,
            SharedEnv = envsResult
        })).ToList();

        // wait for all results to complete
        var results = await Task.WhenAll(allSubmissions);
        
        // update statuses
        foreach (var submission in results)
        {
            _ = await this.UpdateSubmissionStatus(submission);
        }

        return await this.GetById(workspace.Id, job.Id);
    }

    /// <summary>
    /// Submit the task to the cluster
    /// </summary>
    /// <param name="taskClient">The task client reference</param>
    /// <param name="taskInput">The task input</param>
    /// <returns></returns>
    private async Task<TaskSubmissionResult> SubmitTask(IKubernetesTaskClient taskClient, InitTaskInput taskInput)
    {
        var task = taskInput.Task;
        
        // the submission result
        var result = new TaskSubmissionResult
        {
            WorkspaceId = task.WorkspaceId,
            JobId = task.JobId,
            Id = task.Id
        };
        
        try
        {
            // try submitting the task
            result.InitResult = await this.SubmitTaskImpl(taskClient, taskInput);

            // task is submitted
            result.Success = true;
        }
        catch (Exception e)
        {
            // task failed
            result.Success = false;
            
            // record task exception
            result.Exception = e;
        }

        return result;
    }
    
    /// <summary>
    /// Submit the task to the cluster
    /// </summary>
    /// <param name="taskClient">The task client reference</param>
    /// <param name="taskInput">The task input</param>
    /// <returns></returns>
    private async Task<InitTaskResult> SubmitTaskImpl(IKubernetesTaskClient taskClient, InitTaskInput taskInput)
    {
        // submit to the cluster
        var initResult = await taskClient.Submit(taskInput);

        // the task to submit
        var task = taskInput.Task;
        
        // prepare data for the job
        var data = new JobDataMap();
        data.Put(KubernetesWatchConstants.WORKSPACE_ID, task.WorkspaceId);
        data.Put(KubernetesWatchConstants.JOB_ID, task.JobId);
        data.Put(KubernetesWatchConstants.TASK_ID, task.Id);

        // get scheduler
        var scheduler = await this.schedulerFactory.GetScheduler();

        // building a quartz job for monitoring
        var quartzJob = JobBuilder.Create<KubernetesWatchQuartzJob>()
            .WithIdentity(KubernetesWatchQuartzJob.BuildKey(task.JobId, task.Id))
            .RequestRecovery()
            .StoreDurably(false)
            .UsingJobData(data)
            .Build();

        // data for trigger
        var triggerData = new JobDataMap();
        triggerData.Put(KubernetesWatchConstants.TRIGGER_INDEX, "0");
        
        // building an initial trigger
        var initialTrigger = TriggerBuilder.Create()
            .ForJob(quartzJob)
            .UsingJobData(triggerData)
            .StartAt(DateTimeOffset.UtcNow.AddSeconds(KubernetesWatchConstants.TRIGGER_DELAY_SECONDS))
            .WithSimpleSchedule(schedule => schedule.WithMisfireHandlingInstructionFireNow())
            .Build();
        
        // add to quartz
        await scheduler.ScheduleJob(quartzJob, initialTrigger);
        
        return initResult;
    }

    /// <summary>
    /// Updates the task status when the submission is completed (success or failure)
    /// </summary>
    /// <param name="input">The submission result as an input</param>
    /// <returns></returns>
    private async Task<JobTaskModel> UpdateSubmissionStatus(TaskSubmissionResult input)
    {
        // handle status update when submission failed for any reason
        if (!input.Success)
        {
            return await this.taskStatusRepository.CompleteTaskById(input.WorkspaceId, input.JobId, input.Id, new JobTaskCompleteInputModel
            {
                WorkspaceId = input.WorkspaceId,
                JobId = input.Id,
                Id = input.Id,
                Status = JobTaskStatuses.FAILED,
                Message = $"Failed to submit the task: {input.Exception?.Message ?? "Unknown Error"}",
                CompletedAt = DateTime.UtcNow
            });
        }
        
        // mark task as submitted when it's successfully added to the cluster
        return await this.taskStatusRepository.SubmitTaskById(input.WorkspaceId, input.JobId, input.Id, new JobTaskSubmitInputModel
        {
            WorkspaceId = input.WorkspaceId,
            JobId = input.JobId,
            Id = input.Id,
            Message = "Task is submitted to the cluster",
            PendingAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Resolves the environment for submission
    /// </summary>
    /// <param name="input">The input</param>
    /// <returns></returns>
    private async Task<JobTaskEnvModel> ResolveEnvironment(JobSubmissionCreateInput input)
    {
        // get the unique set of names
        var names = input.Manifest.Env.Use.ToHashSet();
        
        // do not consider names of secrets that are subject of overriding 
        names.ExceptWith(input.Manifest.Env.Override.Keys);

        // get the items 
        var items = (await this.secretResolver.ResolveByNames(input.WorkspaceId, input.UserId, names)).ToList();

        // separate workspace secrets
        var workspaceSecrets = items.Where(item => item.Kind == UnifiedSecretKind.Workspace && !item.Disabled).ToList();

        // separate user secrets 
        var userSecrets = items.Where(item => item.Kind == UnifiedSecretKind.User && !item.Disabled).ToList();

        // prepare sets 
        var plain = new Dictionary<string, string>();
        var encrypted = new Dictionary<string, string>();
        
        // add workspaces values first
        foreach (var secret in workspaceSecrets)
        {
            // the target collection
            var target = secret.Encrypted ? encrypted : plain;
            
            // add to the collection
            target[secret.Name] = secret.Value;
        }
        
        // then add user values to override workspace values if any
        foreach (var secret in userSecrets)
        {
            // the target collection
            var target = secret.Encrypted ? encrypted : plain;
            
            // add to the collection
            target[secret.Name] = secret.Value;
        }
        
        // add overriding values to plain values
        foreach (var pairs in input.Manifest.Env.Override)
        {
            plain[pairs.Key] = pairs.Value;
        }
        
        // build result
        return new JobTaskEnvModel
        {
            Plain = plain,
            Encrypted = encrypted
        };
    }
    

    /// <summary>
    /// Parses the given nodes resources into a cluster node resource model
    /// </summary>
    /// <param name="node">The node resource result</param>
    /// <returns></returns>
    private ClusterNodeResourcesModel ParseNodeResult(NodeResourceResult node)
    {
        // parse the CPU
        var cpu = this.resourceParser.ParseToMillicores(node.Cpu) ?? 0;
        
        // parse the memory
        var memory = this.resourceParser.ParseToBytes(node.Memory) ?? 0;

        // parse the nvidia.com/gpu resources
        var nvidiaGpu = this.resourceParser.ParseToGpu(node.NvidiaGpu);

        // parse the amd.com/gpu resources
        var amdGpu = this.resourceParser.ParseToGpu(node.AmdGpu);
        
        return new ClusterNodeResourcesModel
        {
            Name = node.Name,
            CanSchedule = node.CanSchedule,
            Cpu = cpu,
            Memory = memory,
            NvidiaGpu = nvidiaGpu,
            AmdGpu = amdGpu,
            MemoryCpuRatio = cpu > 0 && memory > 0 ? memory * 1.0m / cpu : null,
            MemoryNvidiaGpuRatio = nvidiaGpu > 0 && memory * 1.0m > 0 ? memory / nvidiaGpu : null,
            CpuNvidiaGpuRatio = nvidiaGpu > 0 && cpu > 0 ? cpu * 1.0m / nvidiaGpu : null,
            MemoryAmdGpuRatio = amdGpu > 0 && memory > 0 ? memory * 1.0m / amdGpu : null,
            CpuAmdGpuRatio = amdGpu > 0 && cpu > 0 ? cpu / amdGpu * 1.0m : null
        };
    }

    /// <summary>
    /// Maps the runtime type into a task type
    /// </summary>
    /// <param name="runtimeType">The runtime type</param>
    /// <returns></returns>
    private static string MapTaskType(string runtimeType)
    {
        return runtimeType switch
        {
            "function" => JobTaskTypes.FUNCTION,
            "mpi" => JobTaskTypes.MPI,
            _ => throw ErrorDefinition.Validation(JobErrors.INVALID_RUNTIME_TYPE, $"The {runtimeType} is not valid runtime type").AsException()
        };
    }

    /// <summary>
    /// Performs the initial validation of given input
    /// </summary>
    /// <param name="input">The input to validate</param>
    private async Task ValidateInput(JobSubmissionCreateInput input)
    {
        // validate the scope
        this.validationService.ValidateScope(input.Scope);
        
        // validate the name
        this.validationService.ValidateName(input.Manifest.Name);
        
        // validate the description
        this.validationService.ValidateDescription(input.Manifest.Description);
        
        // validate arguments
        this.validationService.ValidateArgs(input.Manifest.Args);
        
        // validate array of job
        this.validationService.ValidateArray(input.Manifest.Array);
        
        // validate environment
        this.validationService.ValidateEnv(input.Manifest.Env);

        // validate the MPI spec if given
        this.validationService.ValidateMpiSpec(input.Manifest.Spec?.Mpi);
        
        // require the parent object
        await this.validationService.RequireWorkspace(input.WorkspaceId);

        // require valid user
        await this.validationService.RequireUser(input.UserId);
        
        // validate referenced labels
        await this.validationService.ValidateLabels(input.WorkspaceId, input.Manifest.LabelIds);

        // validate referenced git repository
        await this.validationService.ValidateGitRepo(input.WorkspaceId, input.Manifest.GitRepoId);
    }
    
    /// <summary>
    /// Builds a submission input object enriched with default values 
    /// </summary>
    /// <param name="workspaceId">The workspace id</param>
    /// <param name="input">The input object</param>
    /// <returns></returns>
    private static JobSubmissionCreateInput BuildInputWithDefaults(string workspaceId, JobSubmissionCreateInput input)
    {
        // clone the object not to mutate
        input = JsonSerializer.Deserialize<JobSubmissionCreateInput>(JsonSerializer.Serialize(input));

        // initialize parent object
        input.WorkspaceId = workspaceId;
        input.Scope ??= JobScopes.USER;

        // initialize with default if missing
        input.Manifest ??= new JobRunManifestModel();
        input.Manifest.Name ??= string.Empty;
        input.Manifest.Description ??= string.Empty;
        input.Manifest.Args ??= [];
        input.Manifest.Array ??= new JobRunManifestArrayModel();
        input.Manifest.LabelIds ??= [];
        input.Manifest.Env ??= new JobRunManifestEnvModel();
        input.Manifest.Env.Use ??= [];
        input.Manifest.Env.Override ??= new Dictionary<string, string>();
        input.Manifest.Spec ??= new JobRunManifestSpecModel();
        input.Manifest.Resources ??= new JobRunManifestResourcesModel();
        
        // perform basic transformations and apply defaults
        input.Manifest.LabelIds = input.Manifest.LabelIds.Distinct().ToArray();
        input.Manifest.Array.Indexer ??= DEFAULT_JOB_ARRAY_INDEXER;
        input.Manifest.Array.Counter ??= DEFAULT_JOB_ARRAY_COUNT;
        input.Manifest.Array.Replicas ??= DEFAULT_JOB_ARRAY_REPLICAS;
        
        return input;
    }

    /// <summary>
    /// Gets the cluster node resources
    /// </summary>
    /// <param name="clusterConfiguration">The cluster config</param>
    private async Task<List<ClusterNodeResourcesModel>> GetClusterNodes(string clusterConfiguration)
    {
        // create a client to kubernetes
        using var client = new KubernetesClusterClient(clusterConfiguration);

        // get all node resources
        var nodeResources = (await client.GetNodeResources()).ToList();

        // no allocatable nodes at all
        if (nodeResources.Count == 0)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_CLUSTER, "No nodes available").AsException();
        }
        
        // map the resources
        return nodeResources.Select(ParseNodeResult).ToList();
    }
    
    /// <summary>
    /// Deserialize the package runtime into an executable model
    /// </summary>
    /// <param name="runtime">The runtime in json</param>
    /// <returns></returns>
    private static JobTaskRuntimeModel DeserializeRuntime(string runtime)
    {
        try
        {
            // deserialize the runtime into a proper model
            return FromJsonString<JobTaskRuntimeModel>(runtime);
        }
        catch (Exception e)
        {
            throw ErrorDefinition.Validation(JobErrors.INVALID_PACKAGE_RUNTIME, $"The package runtime is not valid: {e.Message}").AsException();
        }
    }
}