export type JobValueType = {
    id: string,
    workspaceId: string,
    workspaceName: string,
    localId: number,
    clusterId: string,
    clusterName: string,
    userId: string,
    userFullName: string,
    name?: string,
    description: string,
    scope: JobScope,
    manifest: string,
    namespace?: string,
    totalTasks: number,
    succeededTasks: number,
    failedTasks: number,
    cancelledTasks: number,
    completedTasks: number,
    status: JobStatus,
    message?: string,
    pendingAt?: string,
    runningAt?: string,
    completedAt?: string,
    created: string,
    updated: string
}

export type JobStatus = 'created' | 'pending' | 'running' | 'partially_succeeded' | 'succeeded' | 'failed' | 'cancelled';

export type JobScope = 'user' | 'workspace';

export interface FilterOptions {
    scope?: JobScope
    status?: JobStatus
    userId?: string
    page: number
    size: number
}

export type JobTaskStatus = 'created' | 'pending' | 'running' | 'succeeded' | 'failed' | 'cancelled';

export type JobTaskType = 'function' | 'mpi';


export type JobTaskValueType = {
    id: string,
    workspaceId: string,
    workspaceName: string,
    jobId: string,
    sequence: number,
    clusterId: string,
    clusterName: string,
    packageId: string,
    packageTemplateReference: string,
    userId: string,
    userFullName: string,
    type: JobTaskType,
    runtime: string,
    args: string,
    arrayReplicas: number,
    arrayIndexer: string,
    memoryRequested?: number,
    cpuRequested?: number,
    nvidiaGpuRequested?: number,
    amdGpuRequested?: number,
    spec: string,
    status: JobTaskStatus,
    message?: string,
    pendingAt?: string,
    runningAt?: string,
    completedAt?: string,
    created: string,
    updated: string
}
