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
import Link from "next/link"
import { Badge } from "../ui/badge"
import Image from "next/image"

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
                  <Image src="/logos/shoc-512x512.png" className="w-6 h-6" width={128} height={128} alt={intl.formatMessage({ id: 'shoc.platform' })} />
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
