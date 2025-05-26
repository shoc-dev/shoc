import { ReactNode } from "react";
import { getByName } from "../../cached-workspace-actions";
import { getClusterByName, getClusterConnectivityById, getClusterPermissionsByName } from "../cached-cluster-actions";
import ErrorScreen from "@/components/error/error-screen";
import ClusterAccessProvider from "@/providers/cluster-access/cluster-access-provider";
import ClusterProvider from "@/providers/cluster/cluster-provider";
import ClusterConnectivityProvider from "@/providers/cluster-connectivity/cluster-connectivity-provider";
import ClusterNodesProvider from "./_providers/cluster-nodes/cluster-nodes-provider";

export default async function SingleClusterLayoutLayout(props: { children: ReactNode, params: Promise<any> }) {
    const params = await props.params;

    const {
        workspaceName,
        clusterName
    } = params;

    const {
        children
    } = props;

    const workspace = await getByName(workspaceName);

    if (workspace.errors) {
        return <ErrorScreen errors={workspace.errors} />
    }

    const [cluster, permissions] = await Promise.all([getClusterByName(workspace.data.id, clusterName), getClusterPermissionsByName(workspace.data.id, clusterName)])

    if (cluster.errors || permissions.errors) {
        return <ErrorScreen errors={cluster.errors || permissions.errors} />
    }

    const connectivity = await getClusterConnectivityById(cluster.data.workspaceId, cluster.data.id)

    if (connectivity.errors) {
        return <ErrorScreen errors={connectivity.errors} />
    }

    return <ClusterProvider initialValue={cluster.data}>
        <ClusterConnectivityProvider initialValue={connectivity.data}>
            <ClusterAccessProvider permissions={permissions.data || []}>
                <ClusterNodesProvider>
                    {children}
                </ClusterNodesProvider>
            </ClusterAccessProvider>
        </ClusterConnectivityProvider>
    </ClusterProvider>
}
