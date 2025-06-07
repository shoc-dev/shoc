using System.Text.Json.Serialization;

namespace Shoc.Core.Kubernetes.Model.Mpi.V2Beta1;

/// <summary>
/// Runtime policy controlling MPIJob clean-up and scheduling behavior.
/// </summary>
public class RunPolicy
{
    [JsonPropertyName("cleanPodPolicy")]
    public string CleanPodPolicy { get; set; }

    [JsonPropertyName("ttlSecondsAfterFinished")]
    public int? TtlSecondsAfterFinished { get; set; }

    [JsonPropertyName("activeDeadlineSeconds")]
    public long? ActiveDeadlineSeconds { get; set; }

    [JsonPropertyName("backoffLimit")]
    public int? BackoffLimit { get; set; }

    [JsonPropertyName("schedulingPolicy")]
    public SchedulingPolicy SchedulingPolicy { get; set; }

    [JsonPropertyName("suspend")]
    public bool? Suspend { get; set; }

    [JsonPropertyName("managedBy")]
    public string ManagedBy { get; set; }
}