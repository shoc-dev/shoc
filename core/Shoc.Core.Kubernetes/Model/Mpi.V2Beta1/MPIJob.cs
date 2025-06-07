using System.Text.Json.Serialization;
using k8s;
using k8s.Models;

namespace Shoc.Core.Kubernetes.Model.Mpi.V2Beta1;

/// <summary>
/// Represents an MPIJob custom resource.
/// </summary>
public class MPIJob : IKubernetesObject<V1ObjectMeta>, ISpec<MPIJobSpec>, IStatus<JobStatus>
{
    /// <inheritdoc/>
    [JsonPropertyName("apiVersion")]
    public string ApiVersion { get; set; }

    /// <inheritdoc/>
    [JsonPropertyName("kind")]
    public string Kind { get; set; }

    /// <inheritdoc/>
    [JsonPropertyName("metadata")]
    public V1ObjectMeta Metadata { get; set; }

    /// <inheritdoc/>
    [JsonPropertyName("spec")]
    public MPIJobSpec Spec { get; set; }

    /// <inheritdoc/>
    [JsonPropertyName("status")]
    public JobStatus Status { get; set; }
}