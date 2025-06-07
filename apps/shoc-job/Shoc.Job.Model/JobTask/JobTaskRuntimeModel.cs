namespace Shoc.Job.Model.JobTask;

/// <summary>
/// The job task runtime
/// </summary>
public class JobTaskRuntimeModel
{
    /// <summary>
    /// The type of runtime
    /// </summary>
    public string Type { get; set; }
    
    /// <summary>
    /// Indicates if supplying extra arguments is permitted
    /// </summary>
    public bool Args { get; set; }
    
    /// <summary>
    /// The user id
    /// </summary>
    public long? Uid { get; set; }
    
    /// <summary>
    /// The username
    /// </summary>
    public string User { get; set; }
    
    /// <summary>
    /// The entrypoint array
    /// </summary>
    public string[] Entrypoint { get; set; }
    
    /// <summary>
    /// The implementation 
    /// </summary>
    public string Implementation { get; set; }
}