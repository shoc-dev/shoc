import { IntlMessageId } from "@/i18n/sources";

export const workspaceNamePattern = /^[a-z\d](?:[a-z\d]|-(?=[a-z\d])){0,38}$/

export const workspaceTypes: { key: string, display: IntlMessageId }[] = [ 
    {
        key: 'individual', 
        display: 'workspaces.types.individual'
    },
    {
        key: 'organization', 
        display: 'workspaces.types.organization'
    }
]

export const workspaceTypesMap = Object.assign({}, ...workspaceTypes.map((entry) => ({[entry.key]: entry.display})));;

export const workspaceStatuses: { key: string, display: IntlMessageId }[] = [ 
    {
        key: 'pending', 
        display: 'workspaces.statuses.pending'
    },
    {
        key: 'active', 
        display: 'workspaces.statuses.active'
    },
    {
        key: 'archived', 
        display: 'workspaces.statuses.archived'
    }
]

export const workspaceStatusesMap = Object.assign({}, ...workspaceStatuses.map((entry) => ({[entry.key]: entry.display})));

export const workspaceRoles: { key: string, display: IntlMessageId }[] = [ 
    {
        key: 'owner', 
        display: 'workspaces.roles.owner'
    },
    {
        key: 'admin', 
        display: 'workspaces.roles.admin'
    },
    {
        key: 'member', 
        display: 'workspaces.roles.member'
    },
    {
        key: 'guest', 
        display: 'workspaces.roles.guest'
    }
]

export const workspaceRolesMap = Object.assign({}, ...workspaceRoles.map((entry) => ({[entry.key]: entry.display})));
