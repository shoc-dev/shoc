"use client"

import { useCallback, useEffect, useState } from "react"
import ErrorScreen from "@/components/error/error-screen";
import { rpc } from "@/server-actions/rpc";
import ClusterCardList from "./cluster-card-list";
import NoClusters from "./no-clusters";
import ClusterAddDialogButton from "./cluster-add-dialog-button";
import { useRouter } from "next/navigation";

export default function WorkspaceClustersClientPage({ workspaceId, workspaceName }: any) {

    const [progress, setProgress] = useState(true);
    const [items, setItems] = useState<any[]>([]);
    const [errors, setErrors] = useState<any[]>([]);
    const router = useRouter();

    const load = useCallback(async (workspaceId: string) => {

        setProgress(true);

        const { data, errors } = await rpc('cluster/workspace-clusters/getAll', { workspaceId })

        if (errors) {
            setErrors(errors);
            setItems([]);
        } else {
            setErrors([]);
            setItems(data)
        }

        setProgress(false);

    }, []);

    useEffect(() => {
        if (!workspaceId) {
            return;
        }

        load(workspaceId);
    }, [load, workspaceId])

    if (errors && errors.length > 0) {
        return <ErrorScreen errors={errors} />
    }

    return <div className="flex flex-col mx-auto w-full h-full space-y-4">
        {items.length > 0 && <div><ClusterAddDialogButton key="add-cluster" workspaceId={workspaceId} disabled={progress} onSuccess={({ name }) => {
            load(workspaceId).then(() => router.push(`/workspaces/${workspaceName}/clusters/${name}`))
        }} /></div>}
        {(progress || items.length > 0) && <ClusterCardList workspaceId={workspaceId} workspaceName={workspaceName} items={items} progress={progress} />}
        {(!progress && items.length === 0) && <div className="py-4 h-full"><NoClusters workspaceId={workspaceId} onCreated={({ name }) => {
            load(workspaceId).then(() => router.push(`/workspaces/${workspaceName}/clusters/${name}`))
        }} /></div>}
    </div>
}