import { ReactNode } from "react";
import { Metadata } from "next";
import { SidebarInset, SidebarProvider } from "@/components/ui/sidebar";
import getIntl from "@/i18n/get-intl";
import AppSidebar from "@/components/sidebar/app-sidebar";

export async function generateMetadata(): Promise<Metadata> {

    const intl = await getIntl();

    return {
        title: intl.formatMessage({ id: 'shoc.platform' })
    }
}

export default async function AppLayout({ children }: { children: ReactNode }) {

    return <SidebarProvider>
        <AppSidebar variant="inset" />
        <SidebarInset>
            {children}
        </SidebarInset>
    </SidebarProvider>
}