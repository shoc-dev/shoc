import { rpc } from "@/server-actions/rpc";
import { cache } from "react";

export const getJobByLocalId = cache((workspaceId: string, localId: number) => {
    return rpc('job/workspace-jobs/getByLocalId', { workspaceId, localId })
})

export const getJobPermissionsByLocalId = cache((workspaceId: string, localId: number) => {
    return rpc('job/workspace-jobs/getPermissionsByLocalId', { workspaceId, localId })
})
