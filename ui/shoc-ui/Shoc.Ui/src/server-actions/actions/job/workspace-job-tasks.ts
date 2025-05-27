import 'server-only';

import { defineServerAction } from '@/server-actions/define';
import { shocClient } from '@/clients/shoc';
import { authenticatedUser } from '@/clients/authenticated';
import clientGuard from '@/clients/client-guard';
import WorkspaceJobTasksClient from '@/clients/shoc/job/workspace-job-tasks-client';

export const getAll = defineServerAction(({ workspaceId, jobId }) => {
    return authenticatedUser(token => clientGuard(() => shocClient(WorkspaceJobTasksClient).getAll(token, workspaceId, jobId)));
});

export const getBySequence = defineServerAction(({ workspaceId, jobId, sequence }) => {
    return authenticatedUser(token => clientGuard(() => shocClient(WorkspaceJobTasksClient).getBySequence(token, workspaceId, jobId, sequence)));
});

export const serverActions = {
    'job/workspace-job-tasks/getAll': getAll,
    'job/workspace-job-tasks/getBySequence': getBySequence,
}
