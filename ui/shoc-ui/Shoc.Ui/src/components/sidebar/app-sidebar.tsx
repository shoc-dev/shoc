"use client"

import NavMain from "@/components/sidebar/nav-main"
import NavUser from "@/components/sidebar/nav-user"
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarRail,
} from "@/components/ui/sidebar"
import useAppMenu from "../menu/use-app-menu"
import { useIntl } from "react-intl"
import { GalleryVerticalEnd } from "lucide-react"
import Link from "next/link"
import { Badge } from "../ui/badge"

export default function AppSidebar({ ...props }: React.ComponentProps<typeof Sidebar>) {

  const menu = useAppMenu();
  const intl = useIntl();

  return (
    <Sidebar collapsible="icon" {...props}>
      <SidebarHeader>
        <SidebarMenu>
          <SidebarMenuItem>
            <SidebarMenuButton size="lg" asChild>
              <Link prefetch={false} href="/workspaces">
                <div className="flex aspect-square size-8 items-center justify-center rounded-lg bg-sidebar-primary text-sidebar-primary-foreground">
                  <GalleryVerticalEnd className="size-4" />
                </div>
                <div className="flex flex-col gap-0.5 leading-none">
                  <span className="font-semibold">{intl.formatMessage({ id: 'shoc.platform' })}</span>
                </div>
                <Badge variant='outline'>Beta</Badge>
              </Link>
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarHeader>
      <SidebarContent>
        <NavMain title={intl.formatMessage({ id: 'shoc.platform' })} items={menu} />
      </SidebarContent>
      <SidebarFooter>
        <NavUser />
      </SidebarFooter>
      <SidebarRail />
    </Sidebar>
  )
}
