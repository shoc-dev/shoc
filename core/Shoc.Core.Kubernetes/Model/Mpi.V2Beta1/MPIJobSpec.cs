using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Shoc.Core.Kubernetes.Model.Mpi.V2Beta1;

/// <summary>
/// Desired specification of an MPIJob.
/// </summary>
public class MPIJobSpec
{
    [JsonPropertyName("slotsPerWorker")]
    public int? SlotsPerWorker { get; set; }

    [JsonPropertyName("runLauncherAsWorker")]
    public bool? RunLauncherAsWorker { get; set; }

    [JsonPropertyName("runPolicy")]
    public RunPolicy RunPolicy { get; set; }

    [JsonPropertyName("mpiReplicaSpecs")]
    public IDictionary<string, ReplicaSpec> MPIReplicaSpecs { get; set; }

    [JsonPropertyName("sshAuthMountPath")]
    public string SSHAuthMountPath { get; set; }

    [JsonPropertyName("launcherCreationPolicy")]
    public string LauncherCreationPolicy { get; set; }

    [JsonPropertyName("mpiImplementation")]
    public string MPIImplementation { get; set; }
}