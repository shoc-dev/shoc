"use client"

import NavMain from "@/components/sidebar/nav-main"
import NavUser from "@/components/sidebar/nav-user"
import WorkspaceSwitcher from "@/components/sidebar/workspace-switcher"
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarHeader,
  SidebarRail,
} from "@/components/ui/sidebar"
import useWorkspaceMenu from "@/components/menu/use-workspace-menu"

export default function WorkspaceSidebar({ ...props }: React.ComponentProps<typeof Sidebar>) {
  
  const menu = useWorkspaceMenu();
  
  return (
    <Sidebar collapsible="icon" {...props}>
      <SidebarHeader>
        <WorkspaceSwitcher />
      </SidebarHeader>
      <SidebarContent>
        <NavMain items={menu} />
      </SidebarContent>
      <SidebarFooter>
        <NavUser />
      </SidebarFooter>
      <SidebarRail />
    </Sidebar>
  )
}
