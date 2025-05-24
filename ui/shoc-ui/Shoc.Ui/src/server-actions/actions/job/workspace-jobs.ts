import 'server-only';

import { defineServerAction } from '@/server-actions/define';
import { shocClient } from '@/clients/shoc';
import { authenticatedUser } from '@/clients/authenticated';
import clientGuard from '@/clients/client-guard';
import WorkspaceJobsClient from '@/clients/shoc/job/workspace-jobs-client';

export const getBy = defineServerAction(({ workspaceId, filter }) => {
    return authenticatedUser(token => clientGuard(() => shocClient(WorkspaceJobsClient).getBy(token, workspaceId, filter)));
});

export const getByLocalId = defineServerAction(({ workspaceId, localId }) => {
    return authenticatedUser(token => clientGuard(() => shocClient(WorkspaceJobsClient).getByLocalId(token, workspaceId, localId)));
});

export const getPermissionsByLocalId = defineServerAction(({ workspaceId, localId }) => {
    return authenticatedUser(token => clientGuard(() => shocClient(WorkspaceJobsClient).getPermissionsByLocalId(token, workspaceId, localId)));
});

export const serverActions = {
    'job/workspace-jobs/getBy': getBy,
    'job/workspace-jobs/getByLocalId': getByLocalId,
    'job/workspace-jobs/getPermissionsByLocalId': getPermissionsByLocalId
}
