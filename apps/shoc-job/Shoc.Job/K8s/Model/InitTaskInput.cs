using Shoc.Job.Model.Job;
using Shoc.Job.Model.JobTask;

namespace Shoc.Job.K8s.Model;

/// <summary>
/// The task initialization input
/// </summary>
public class InitTaskInput
{
    /// <summary>
    /// The job instance
    /// </summary>
    public JobModel Job { get; set; }
    
    /// <summary>
    /// The job task instance
    /// </summary>
    public JobTaskModel Task { get; set; }
    
    /// <summary>
    /// The manifest spec
    /// </summary>
    public JobRunManifestSpecModel Spec { get; set; }
    
    /// <summary>
    /// The runtime of the task
    /// </summary>
    public JobTaskRuntimeModel Runtime { get; set; }
    
    /// <summary>
    /// The args of the task
    /// </summary>
    public JobTaskArgsModel Args { get; set; }
    
    /// <summary>
    /// The namespace for submission
    /// </summary>
    public string Namespace { get; set; }
    
    /// <summary>
    /// The service account name
    /// </summary>
    public string ServiceAccount { get; set; }
    
    /// <summary>
    /// The pull secret
    /// </summary>
    public InitPullSecretResult PullSecret { get; set; }
    
    /// <summary>
    /// The shared environment
    /// </summary>
    public InitSharedEnvsResult SharedEnv { get; set; }
    
    /// <summary>
    /// The shared runtime
    /// </summary>
    public InitSharedRuntimeResult SharedRuntime { get; set; }
}