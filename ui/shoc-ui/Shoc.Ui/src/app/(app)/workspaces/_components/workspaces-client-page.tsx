"use client"

import { useCallback, useEffect, useState } from "react"
import WorkspaceAddDialogButton from "./workspace-add-dialog-button";
import ErrorScreen from "@/components/error/error-screen";
import { rpc } from "@/server-actions/rpc";
import WorkspaceCardList from "./workspace-card-list";
import { useIntl } from "react-intl";
import BasicHeader from "@/components/general/basic-header";
import InvitationsButton from "./invitations-button";

export default function WorkspacesClientPage() {

    const [progress, setProgress] = useState(true);
    const [items, setItems] = useState<any[]>([]);
    const [errors, setErrors] = useState<any[]>([]);
    const intl = useIntl();

    const load = useCallback(async () => {

        setProgress(true);

        const { data, errors } = await rpc('workspace/user-workspaces/getAll', {})

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
        load();
    }, [load])

    if (errors && errors.length > 0) {
        return <ErrorScreen errors={errors} />
    }

    return <div className="flex w-full h-full flex-col gap-4 p-4 lg:gap-6 lg:p-6">
        <BasicHeader 
            title={intl.formatMessage({id: 'workspaces'})}
            actions={[<div key="workspace-header-operations" className="flex space-x-1">
                {items.length > 0 && <WorkspaceAddDialogButton key="add-workspace" disabled={progress} onSuccess={() => load()} />}
                <InvitationsButton />
            </div>]}
        />
        <WorkspaceCardList items={items} progress={progress} />            
    </div>
}