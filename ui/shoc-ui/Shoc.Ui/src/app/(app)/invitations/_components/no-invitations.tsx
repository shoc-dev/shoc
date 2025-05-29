"use client"

import { useIntl } from "react-intl";
import WorkspacesNavigationButton from "./workspaces-navigation-button";

export default function NoInvitations(){
    const intl = useIntl();

    return <div className="flex flex-1 items-center justify-center rounded-lg border border-dashed shadow-xs">
    <div className="flex flex-col items-center gap-1 text-center">
        <h3 className="text-2xl font-bold tracking-tight">{intl.formatMessage({id: 'workspaces.invitations.noInvitations'})}</h3>
        <p className="text-sm text-muted-foreground">{intl.formatMessage({id: 'workspaces.invitations.noInvitations.note'})}</p>
        <WorkspacesNavigationButton className="mt-5" />
    </div>
</div>
}