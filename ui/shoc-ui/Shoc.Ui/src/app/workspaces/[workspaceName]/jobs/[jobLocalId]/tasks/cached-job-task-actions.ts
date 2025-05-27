import { rpc } from "@/server-actions/rpc";
import { cache } from "react";

export const getTaskBySequence = cache((workspaceId: string, jobId: string, sequence: number) => {
    return rpc('job/workspace-job-tasks/getBySequence', { workspaceId, jobId, sequence })
})
