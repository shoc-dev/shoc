namespace Shoc.Job.Model.Job;

/// <summary>
/// The job run manifest specification of MPI model
/// </summary>
public class JobRunManifestSpecMpiModel
{
    /// <summary>
    /// The launcher specification
    /// </summary>
    public JobRunManifestSpecMpiLauncherModel Launcher { get; set; }
    
    /// <summary>
    /// The workers specification
    /// </summary>
    public JobRunManifestSpecMpiWorkersModel Workers { get; set; }
}