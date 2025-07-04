using System;
using System.Threading.Tasks;
using Shoc.Job.K8s.Model;
using Shoc.Job.Model.Job;
using Shoc.Job.Model.JobTask;

namespace Shoc.Job.K8s.JobClients;

/// <summary>
/// The kubernetes job client
/// </summary>
public interface IKubernetesJobClient : IDisposable
{
    /// <summary>
    /// Initializes the namespace object for the job
    /// </summary>
    /// <param name="job">The job object</param>
    /// <returns></returns>
    public Task<InitNamespaceResult> InitNamespace(JobModel job);

    /// <summary>
    /// Initializes the k8s essentials for the given job
    /// </summary>
    /// <param name="job">The job object</param>
    /// <returns></returns>
    public Task<InitServiceAccountResult> InitServiceAccount(JobModel job);

    /// <summary>
    /// Initialize the shared environment such as secrets and config maps
    /// </summary>
    /// <param name="job">The job object</param>
    /// <param name="envs">The environment input</param>
    /// <returns></returns>
    public Task<InitSharedEnvsResult> InitSharedEnvironment(JobModel job, JobTaskEnvModel envs);

    /// <summary>
    /// Initialize the pull secret for the target image
    /// </summary>
    /// <param name="job">The job object</param>
    /// <param name="packageReference">The package reference</param>
    /// <returns></returns>
    public Task<InitPullSecretResult> InitPullSecret(JobModel job, JobTaskPackageReferenceModel packageReference);

    /// <summary>
    /// Initializes the shared runtime
    /// </summary>
    /// <param name="job">The job object</param>
    /// <returns></returns>
    public Task<InitSharedRuntimeResult> InitSharedRuntime(JobModel job);

    /// <summary>
    /// Performs a K8s action with namespace cleanup fallback
    /// </summary>
    /// <param name="ns">The namespace</param>
    /// <param name="action">The action to perform</param>
    /// <typeparam name="TResult">The result type</typeparam>
    /// <returns></returns>
    public Task<TResult> WithCleanup<TResult>(string ns, Func<Task<TResult>> action);
}