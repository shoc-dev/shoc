using System.Text.Json.Serialization;
using k8s.Models;

namespace Shoc.Core.Kubernetes.Model.Mpi.V2Beta1;

/// <summary>
/// Status of each MPI replica type.
/// </summary>
public class ReplicaStatus
{
    [JsonPropertyName("active")]
    public int? Active { get; set; }

    [JsonPropertyName("succeeded")]
    public int? Succeeded { get; set; }

    [JsonPropertyName("failed")]
    public int? Failed { get; set; }

    [JsonPropertyName("labelSelector")]
    public V1LabelSelector LabelSelector { get; set; }

    [JsonPropertyName("selector")]
    public string Selector { get; set; }
}