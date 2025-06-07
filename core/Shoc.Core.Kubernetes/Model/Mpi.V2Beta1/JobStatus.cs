using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Shoc.Core.Kubernetes.Model.Mpi.V2Beta1;

/// <summary>
/// Current observed state of the MPIJob.
/// </summary>
public class JobStatus
{
    [JsonPropertyName("conditions")]
    public List<JobCondition> Conditions { get; set; }

    [JsonPropertyName("replicaStatuses")]
    public IDictionary<string, ReplicaStatus> ReplicaStatuses { get; set; }

    [JsonPropertyName("startTime")]
    public DateTime? StartTime { get; set; }

    [JsonPropertyName("completionTime")]
    public DateTime? CompletionTime { get; set; }

    [JsonPropertyName("lastReconcileTime")]
    public DateTime? LastReconcileTime { get; set; }
}