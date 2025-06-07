using System.Text.Json.Serialization;
using k8s.Models;

namespace Shoc.Core.Kubernetes.Model.Mpi.V2Beta1;

/// <summary>
/// Desired state of an MPI replica.
/// </summary>
public class ReplicaSpec
{
    [JsonPropertyName("replicas")]
    public int? Replicas { get; set; }

    [JsonPropertyName("template")]
    public V1PodTemplateSpec Template { get; set; }

    [JsonPropertyName("restartPolicy")]
    public string RestartPolicy { get; set; }
}