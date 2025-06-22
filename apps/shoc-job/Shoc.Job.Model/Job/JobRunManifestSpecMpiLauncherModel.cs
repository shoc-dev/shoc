namespace Shoc.Job.Model.Job;

/// <summary>
/// The job run manifest specification of MPI launcher model
/// </summary>
public class JobRunManifestSpecMpiLauncherModel
{
    /// <summary>
    /// Indicates if dedicated launcher should be started for the MPI Job
    /// </summary>
    public bool Dedicated { get; set; }
    
    /// <summary>
    /// The launcher resources
    /// </summary>
    public JobRunManifestResourcesModel Resources { get; set; }
}