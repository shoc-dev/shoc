using System;
using System.Text.Json.Serialization;

namespace Shoc.Core.Kubernetes.Model.Mpi.V2Beta1;

/// <summary>
/// Condition details for the MPIJob's lifecycle.
/// </summary>
public class JobCondition
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("reason")]
    public string Reason { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("lastUpdateTime")]
    public DateTime? LastUpdateTime { get; set; }

    [JsonPropertyName("lastTransitionTime")]
    public DateTime? LastTransitionTime { get; set; }
}