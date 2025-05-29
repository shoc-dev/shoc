import { ReactNode } from "react";
import { getByName, getPermissionsByName } from "./cached-workspace-actions";
import ErrorScreen from "@/components/error/error-screen";
import WorkspaceAccessProvider from "@/providers/workspace-access/workspace-access-provider";
import { Metadata } from "next";
import { SidebarInset, SidebarProvider } from "@/components/ui/sidebar";
import WorkspaceProvider from "@/providers/workspace/workspace-provider";
import WorkspaceSidebar from "@/app/workspaces/[workspaceName]/_components/workspace-sidebar";

export async function generateMetadata(props: { params: Promise<any> }): Promise<Metadata> {
    const params = await props.params;

    const {
        workspaceName
    } = params;

    const { data } = await getByName(workspaceName);

    return {
        description: data?.description || ''
    }
}

export default async function SingleWorkspaceLayout(props: { children: ReactNode, params: Promise<any> }) {
    const params = await props.params;

    const {
        workspaceName
    } = params;

    const {
        children
    } = props;

    const [workspace, permissions] = await Promise.all([getByName(workspaceName), getPermissionsByName(workspaceName)])

    if (workspace.errors || permissions.errors) {
        return <ErrorScreen errors={workspace.errors || permissions.errors} />
    }

    return <WorkspaceProvider initialValue={workspace.data}>
        <WorkspaceAccessProvider permissions={permissions.data || []}>
            <SidebarProvider>
                <WorkspaceSidebar variant="inset" />
                <SidebarInset>
                    {children}
                </SidebarInset>
            </SidebarProvider>
        </WorkspaceAccessProvider>
    </WorkspaceProvider>
}