namespace Shoc.Job.Quartz;

/// <summary>
/// The Kubernetes watch constants
/// </summary>
public static class KubernetesWatchConstants
{
    /// <summary>
    /// The prefix of watch job
    /// </summary>
    public const string WATCH_JOB_PREFIX = "monitor";
    
    /// <summary>
    /// The workspace id
    /// </summary>
    public const string WORKSPACE_ID = "workspaceId";

    /// <summary>
    /// The job id
    /// </summary>
    public const string JOB_ID = "jobId";

    /// <summary>
    /// The task id
    /// </summary>
    public const string TASK_ID = "taskId";

    /// <summary>
    /// The trigger index
    /// </summary>
    public const string TRIGGER_INDEX = "triggerIndex";

    /// <summary>
    /// Delay the execution of the first job by given seconds
    /// </summary>
    public const int TRIGGER_DELAY_SECONDS = 5;

    /// <summary>
    /// Triggers to automatically apply for watching job
    /// Later can be configured dynamically like this: Trigger in 5 sec, then in 10, then 20, etc
    /// </summary>
    public static readonly int[] NEXT_TRIGGERS = [5];
}