import { serverActions as index } from './actions'
import { serverActions as auth } from './actions/auth'
import { serverActions as userWorkspaces } from './actions/workspace/user-workspaces'
import { serverActions as userWorkspaceMembers } from './actions/workspace/user-workspace-members'
import { serverActions as userWorkspaceInvitations } from './actions/workspace/user-workspace-invitations'
import { serverActions as userInvitations } from './actions/workspace/user-invitations'
import { serverActions as workspaceClusters } from './actions/cluster/workspace-clusters'
import { serverActions as workspaceClusterInstance } from './actions/cluster/workspace-cluster-instance'
import { serverActions as workspaceSecrets } from './actions/secret/workspace-secrets'
import { serverActions as workspaceUserSecrets } from './actions/secret/workspace-user-secrets'
import { serverActions as templates } from './actions/template/templates'
import { serverActions as workspaceJobs } from './actions/job/workspace-jobs'
import { serverActions as workspaceJobTasks } from './actions/job/workspace-job-tasks'

const allRpc = {
    ...index,
    ...auth,
    ...userWorkspaces,
    ...userWorkspaceMembers,
    ...userWorkspaceInvitations,
    ...userInvitations,
    ...workspaceClusters,
    ...workspaceClusterInstance,
    ...workspaceSecrets,
    ...workspaceUserSecrets,
    ...templates,
    ...workspaceJobs,
    ...workspaceJobTasks
}

export default allRpc;
