using System.Collections.Generic;
using System.Text.Json.Serialization;
using k8s;
using k8s.Models;

namespace Shoc.Core.Kubernetes.Model.Mpi.V2Beta1;

/// <summary>
/// Represents a list of MPIJob resources.
/// </summary>
public class MPIJobList : IKubernetesObject
{
    [JsonPropertyName("apiVersion")]
    public string ApiVersion { get; set; }

    [JsonPropertyName("kind")]
    public string Kind { get; set; }

    [JsonPropertyName("metadata")]
    public V1ListMeta Metadata { get; set; }

    [JsonPropertyName("items")]
    public List<MPIJob> Items { get; set; }
}