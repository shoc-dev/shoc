"use client"

import { useIntl } from "react-intl";
import WorkspaceAddDialogButton from "./workspace-add-dialog-button";

export default function NoWorkspace(){
    const intl = useIntl();

    return <div className="flex flex-1 h-full items-center justify-center rounded-lg border border-dashed shadow-xs">
    <div className="flex flex-col items-center gap-1 text-center">
        <h3 className="text-2xl font-bold tracking-tight">{intl.formatMessage({id: 'workspaces.noWorkspaces'})}</h3>
        <p className="text-sm text-muted-foreground">{intl.formatMessage({id: 'workspaces.noWorkspaces.note'})}</p>
        <WorkspaceAddDialogButton className="mt-4" />
    </div>
</div>
}