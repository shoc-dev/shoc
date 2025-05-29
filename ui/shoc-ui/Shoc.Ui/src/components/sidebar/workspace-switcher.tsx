"use client"

import { ChevronsUpDown, MoreHorizontalIcon } from "lucide-react"

import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import {
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  useSidebar,
} from "@/components/ui/sidebar"
import OrganizationIcon from "../icons/organization-icon"
import { useCallback, useEffect, useState } from "react"
import { rpc } from "@/server-actions/rpc"
import { useIntl } from "react-intl"
import { useRouter } from "next/navigation"
import useWorkspace from "@/providers/workspace/use-workspace"
import UsersIcon from "../icons/users-icon"
import { workspaceRolesMap } from "@/well-known/workspace"

export default function WorkspaceSwitcher() {
  const { isMobile } = useSidebar()
  const [items, setItems] = useState<{ name: string, type: string }[]>([]);
  const [progress, setProgress] = useState(true);
  const intl = useIntl();
  const router = useRouter();
  const { value: workspace } = useWorkspace();

  const load = useCallback(async () => {

    setProgress(true);

    const { data, errors } = await rpc('workspace/user-workspaces/getAll', {})
    if (errors) {
      setItems([]);
    } else {
      setItems(data)
    }

    setProgress(false);

  }, []);

  useEffect(() => {
    load();
  }, [load])

  return (
    <SidebarMenu>
      <SidebarMenuItem>
        <DropdownMenu modal={false}>
          <DropdownMenuTrigger asChild disabled={progress}>
            <SidebarMenuButton
              size="lg"
              className="data-[state=open]:bg-sidebar-accent data-[state=open]:text-sidebar-accent-foreground"
            >
              <div className="flex aspect-square size-8 items-center justify-center rounded-lg bg-sidebar-primary text-sidebar-primary-foreground">

                {workspace.type === 'organization' && <OrganizationIcon className="size-4 " />}
                {workspace.type === 'individual' && <UsersIcon className="size-4" />}
              </div>
              <div className="grid flex-1 text-left text-sm leading-tight">
                <span className="truncate font-semibold">
                  {workspace?.name}
                </span>
                <span className="truncate text-xs">
                  {intl.formatMessage({ id: 'workspaces.labels.role' })}: {intl.formatMessage({ id: workspaceRolesMap[workspace.role] })}
                </span>
              </div>
              <ChevronsUpDown className="ml-auto" />
            </SidebarMenuButton>
          </DropdownMenuTrigger>
          <DropdownMenuContent
            className="w-[--radix-dropdown-menu-trigger-width] min-w-56 rounded-lg"
            align="start"
            side={isMobile ? "bottom" : "right"}
            sideOffset={4}
          >
            <DropdownMenuLabel className="text-xs text-muted-foreground">
              {intl.formatMessage({ id: 'workspaces' })}
            </DropdownMenuLabel>
            {items.map((item, index) => (
              <DropdownMenuItem
                key={item.name}
                onClick={() => router.push(`/workspaces/${item.name}`)}
                disabled={progress}
                className="gap-2 p-2"
              >
                <div className="flex size-6 items-center justify-center rounded-sm border">
                  {item.type === 'organization' && <OrganizationIcon className="size-4 shrink-0" />}
                  {item.type === 'individual' && <UsersIcon className="size-4 shrink-0" />}
                </div>
                {item.name}
              </DropdownMenuItem>
            ))}
            <DropdownMenuSeparator />
            <DropdownMenuItem className="gap-2 p-2" disabled={progress} onClick={() => router.push(`/workspaces`)}>
              <div className="flex size-6 items-center justify-center rounded-md border bg-background">
                <MoreHorizontalIcon className="size-4" />
              </div>
              <div className="font-medium text-muted-foreground">{intl.formatMessage({ id: 'global.filters.all' })}</div>
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </SidebarMenuItem>
    </SidebarMenu>
  )
}
