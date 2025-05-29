import ClusterIcon from "@/components/icons/cluster-icon";
import HomeIcon from "@/components/icons/home-icon";
import UsersIcon from "@/components/icons/users-icon";
import useWorkspaceAccess from "@/providers/workspace-access/use-workspace-access";
import { MixerHorizontalIcon, PlayIcon } from "@radix-ui/react-icons";
import { usePathname } from "next/navigation";
import { useMemo } from "react";
import { useIntl } from "react-intl";
import { WorkspacePermissions } from "@/well-known/workspace-permissions";
import { KeyRoundIcon } from "lucide-react";
import useWorkspace from "@/providers/workspace/use-workspace";

export default function useWorkspaceMenu(){

    const intl = useIntl();
    const pathname = usePathname();
    const { hasAny } = useWorkspaceAccess();
    const { value: { name } } = useWorkspace();

    const all = useMemo(() => [
        { 
            path: `/workspaces/${name}`, 
            title: intl.formatMessage({ id: 'workspaces.sidebar.dashboard' }), 
            icon: HomeIcon,
            visible: hasAny([WorkspacePermissions.WORKSPACE_VIEW])
        },
        { 
            path: `/workspaces/${name}/jobs`, 
            title: intl.formatMessage({ id: 'workspaces.sidebar.jobs' }), 
            icon: PlayIcon,
            visible: hasAny([WorkspacePermissions.WORKSPACE_VIEW])
        },
        { 
            path: `/workspaces/${name}/clusters`, 
            title: intl.formatMessage({ id: 'workspaces.sidebar.clusters' }), 
            icon: ClusterIcon,
            visible: hasAny([WorkspacePermissions.WORKSPACE_LIST_CLUSTERS])
        },
        { 
            path: `/workspaces/${name}/secrets`, 
            title: intl.formatMessage({ id: 'workspaces.sidebar.secrets' }), 
            icon: KeyRoundIcon,
            visible: hasAny([WorkspacePermissions.WORKSPACE_LIST_SECRETS, WorkspacePermissions.WORKSPACE_LIST_USER_SECRETS]),
            altPaths: [`/workspaces/${name}/secrets/workspace-secrets`, `/workspaces/${name}/secrets/user-secrets`],
            items: [
                {
                    path: `/workspaces/${name}/secrets/workspace-secrets`,
                    title: intl.formatMessage({ id: 'secrets.menu.workspaceSecrets' }),
                    visible: hasAny([WorkspacePermissions.WORKSPACE_LIST_SECRETS]),
                },
                {
                    path: `/workspaces/${name}/secrets/user-secrets`,
                    title: intl.formatMessage({ id: 'secrets.menu.userSecrets' }),
                    visible: hasAny([WorkspacePermissions.WORKSPACE_LIST_USER_SECRETS]),
                }
            ]
        },
        { 
            path: `/workspaces/${name}/members`, 
            title: intl.formatMessage({ id: 'workspaces.sidebar.members' }), 
            icon: UsersIcon,
            visible: hasAny([WorkspacePermissions.WORKSPACE_LIST_MEMBERS, WorkspacePermissions.WORKSPACE_LIST_INVITATIONS]),
            items: [
                {
                    path: `/workspaces/${name}/members`,
                    title: intl.formatMessage({ id: 'workspaces.members.menu.members' }),
                    visible: hasAny([WorkspacePermissions.WORKSPACE_LIST_MEMBERS]),
                },
                {
                    path: `/workspaces/${name}/members/invitations`,
                    title: intl.formatMessage({ id: 'workspaces.members.menu.invitations' }),
                    visible: hasAny([WorkspacePermissions.WORKSPACE_LIST_INVITATIONS]),
                }
            ],
            altPaths: [`/workspaces/${name}/members`, `/workspaces/${name}/members/invitations`]
        },
        { 
            path: `/workspaces/${name}/settings`, 
            title: intl.formatMessage({ id: 'workspaces.sidebar.settings' }), 
            icon: MixerHorizontalIcon,
            visible: hasAny([WorkspacePermissions.WORKSPACE_UPDATE])
        }
    ], [name, hasAny, intl])

    const menu = useMemo(() => all.filter(item => item.visible).map(item => ({
        title: item.title,
        path: item.path,
        icon: item.icon,
        active: item.path === pathname || item.altPaths?.includes(pathname || ''),
        items: item.items?.filter(subItem => subItem.visible).map(subItem => ({
            title: subItem.title,
            path: subItem.path,
            active: subItem.path === pathname
        }))
    })), [all, pathname])


    return menu
}