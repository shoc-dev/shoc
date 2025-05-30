"use client"

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { useIntl } from "react-intl";
import UsersIcon from "@/components/icons/users-icon";
import OrganizationIcon from "@/components/icons/organization-icon";
import KeyIcon from "@/components/icons/key-icon";
import { useState } from "react";
import { Check, X } from "lucide-react";
import InvitationDeclineDialog from "./invitation-decline-dialog";
import InvitationAcceptDialog from "./invitation-accept-dialog";
import { workspaceRolesMap, workspaceTypesMap } from "@/well-known/workspace";

export default function InvitationCard({ invitation, onAccepted = () => { }, onDeclined = () => { } }: any) {

    const [acceptActive, setAcceptActive] = useState(false);
    const [declineActive, setDeclineActive] = useState(false);
    const intl = useIntl();

    return <Card className="w-full">
        <InvitationAcceptDialog
            item={invitation}
            open={acceptActive}
            onClose={() => setAcceptActive(false)}
            onSuccess={result => onAccepted(result)}
        />
        <InvitationDeclineDialog
            item={invitation}
            open={declineActive}
            onClose={() => setDeclineActive(false)}
            onSuccess={result => onDeclined(result)}
        />

        <CardHeader>
            <CardTitle className="truncate leading-normal">{invitation.workspaceName}</CardTitle>
            <CardDescription className="truncate text-balance">{invitation.workspaceDescription}</CardDescription>
        </CardHeader>

        <CardContent className="h-full">
            <div>
                <Badge variant="secondary">
                    {invitation.workspaceType === 'individual' && <UsersIcon className="w-4 h-4 mr-1" />}
                    {invitation.workspaceType === 'organization' && <OrganizationIcon className="w-4 h-4 mr-1" />}
                    {intl.formatMessage({ id: 'workspaces.labels.type' })}: {intl.formatMessage({ id: workspaceTypesMap[invitation.workspaceType] })}
                </Badge>
                <Badge variant="secondary" className="ml-2">
                    <KeyIcon className="w-4 h-4 mr-1" />
                    {intl.formatMessage({ id: 'workspaces.labels.role' })}: {intl.formatMessage({ id: workspaceRolesMap[invitation.role] })}
                </Badge>
            </div>
        </CardContent>
        <CardFooter>

            <div className="flex space-x-2">
                <Button key="accept" variant="outline" onClick={() => setAcceptActive(true)}>
                    {intl.formatMessage({ id: 'global.actions.accept' })} <Check className="w-4 h-4 ml-2" />
                </Button>
                <Button key="decline" variant="destructive" onClick={() => setDeclineActive(true)}>
                    {intl.formatMessage({ id: 'global.actions.decline' })} <X className="w-4 h-4 ml-2" />
                </Button>
            </div>
        </CardFooter>
    </Card>
}