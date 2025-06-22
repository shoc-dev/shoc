"use client"

import ProLayout from "@ant-design/pro-layout"
import React, { ReactNode } from "react"
import useMenu from "./menu"
import { usePathname } from "next/navigation";
import Link from "next/link";
import ConsoleHeaderRightMenu from "./console-header-right-menu";
import useTitle from "@/providers/title-provider/use-title";
import Image from "next/image";

const defaultTitle = 'Shoc Platform';

export default function ConsoleLayout({ children } : { children: ReactNode }){
  
   const menu = useMenu();
   const pathname = usePathname();
   const { title } = useTitle();

   return (
      <>
         <ProLayout 
            pageTitleRender={() => title ?  `${title} - ${defaultTitle}` : defaultTitle}
            layout="mix"
            locale="en-US"
            title={defaultTitle} 
            logo={false}
            headerTitleRender={(_, title) => <div style={{ display: 'flex' }}>
               <Link href="/" prefetch={false}>{title}</Link>
            </div>}
            siderWidth={270}
            menu={{ request: async () => menu }}
            location={{ pathname: pathname || undefined }}
            menuItemRender={(item, dom) => {
               return item.path ? <Link href={item.path}>{dom}</Link> : dom;
            }}
            subMenuItemRender={(item, dom) => {
               return item.path ? <Link href={item.path}>{dom}</Link> : dom;
            }}
            actionsRender={() => <ConsoleHeaderRightMenu />}
         >
               {children}
         </ProLayout>
      </>
   )
}
