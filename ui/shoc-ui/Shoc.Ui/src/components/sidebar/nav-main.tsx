"use client"

import {
  SidebarGroup,
  SidebarGroupLabel,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarMenuSub,
  SidebarMenuSubButton,
  SidebarMenuSubItem
} from "@/components/ui/sidebar"
import { FC } from "react"
import Link from "next/link"
import { Collapsible } from "../ui/collapsible"
import { CollapsibleContent, CollapsibleTrigger } from "@radix-ui/react-collapsible"
import { ChevronRight } from "lucide-react"

export default function NavMain({
  title,
  items,
}: {
  title?: string,
  items: {
    title: string
    path: string
    icon?: FC
    active?: boolean
    items?: {
      title: string
      path: string
    }[]
  }[]
}) {
  return (
    <SidebarGroup>
      {title && <SidebarGroupLabel>{title}</SidebarGroupLabel>}
      <SidebarMenu>
        {
          items.map((item) => (item.items || []).length === 0 ? <SidebarMenuItem key={item.path}>
            <SidebarMenuButton asChild>
              <Link prefetch={false} href={item.path}>
                {item.icon && <item.icon />}
                <span>{item.title}</span>
              </Link>
            </SidebarMenuButton>
          </SidebarMenuItem>
            : <Collapsible
              key={item.path}
              asChild
              defaultOpen={item.active}
              className="group/collapsible"
            >
              <SidebarMenuItem>
                <CollapsibleTrigger asChild>
                  <SidebarMenuButton tooltip={item.title}>
                    {item.icon && <item.icon />}
                    <span>{item.title}</span>
                    <ChevronRight className="ml-auto transition-transform duration-200 group-data-[state=open]/collapsible:rotate-90" />
                  </SidebarMenuButton>
                </CollapsibleTrigger>
                <CollapsibleContent>
                  <SidebarMenuSub>
                    {item.items?.map((subItem) => (
                      <SidebarMenuSubItem key={subItem.path}>
                        <SidebarMenuSubButton asChild>
                          <Link prefetch={false} href={subItem.path}>
                            <span>{subItem.title}</span>
                          </Link>
                        </SidebarMenuSubButton>
                      </SidebarMenuSubItem>
                    ))}
                  </SidebarMenuSub>
                </CollapsibleContent>
              </SidebarMenuItem>
            </Collapsible>)
        }
      </SidebarMenu>
    </SidebarGroup>
  )
}
