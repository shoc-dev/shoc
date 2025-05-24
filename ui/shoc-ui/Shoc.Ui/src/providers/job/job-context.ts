import { JobScope, JobStatus } from "@/app/workspaces/[workspaceName]/jobs/_components/types";
import { createContext } from "react";

const JobContext = createContext<JobContextValueType | any>({});

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

export type JobContextValueType = {
    value: JobValueType,
    initialValue: JobValueType,
    load: () => Promise<any>,
    loading: boolean,
    errors: any[]
}

export default JobContext;