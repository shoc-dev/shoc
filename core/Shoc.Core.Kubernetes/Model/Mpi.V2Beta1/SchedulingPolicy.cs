using System.Text.Json.Serialization;
using k8s.Models;

namespace Shoc.Core.Kubernetes.Model.Mpi.V2Beta1;

/// <summary>
/// Gang-scheduling and related resource requirements.
/// </summary>
public class SchedulingPolicy
{
    [JsonPropertyName("minAvailable")]
    public int? MinAvailable { get; set; }

    [JsonPropertyName("queue")]
    public string Queue { get; set; }

    [JsonPropertyName("minResources")]
    public V1APIResourceList MinResources { get; set; }

    [JsonPropertyName("priorityClass")]
    public string PriorityClass { get; set; }

    [JsonPropertyName("scheduleTimeoutSeconds")]
    public int? ScheduleTimeoutSeconds { get; set; }
}