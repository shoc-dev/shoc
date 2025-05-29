"use client"

import { useCallback, useEffect, useState } from "react"
import ErrorScreen from "@/components/error/error-screen";
import { rpc } from "@/server-actions/rpc";
import { useIntl } from "react-intl";
import BasicHeader from "@/components/general/basic-header";
import WorkspacesNavigationButton from "./workspaces-navigation-button";
import InvitationsCardList from "./invitations-card-list";

export default function InvitationsClientPage() {

    const [progress, setProgress] = useState(true);
    const [items, setItems] = useState<any[]>([]);
    const [errors, setErrors] = useState<any[]>([]);
    const intl = useIntl();

    const load = useCallback(async () => {

        setProgress(true);

        const { data, errors } = await rpc('workspace/user-invitations/getAll', {})

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
            title={intl.formatMessage({ id: 'workspaces.invitations' })}
            actions={[<div key="workspace-btn-actions">
                {items.length > 0 && <WorkspacesNavigationButton key="workspaces-btn" />}
            </div>]}
        />
        <InvitationsCardList items={items} progress={progress} onUpdate={() => load()} />
    </div>
}