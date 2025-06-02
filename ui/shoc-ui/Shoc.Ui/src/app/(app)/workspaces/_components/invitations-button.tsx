"use client"

import SpinnerIcon from "@/components/icons/spinner-icon";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { rpc } from "@/server-actions/rpc";
import { SendIcon } from "lucide-react";
import { useRouter } from "next/navigation";
import { useCallback, useEffect, useState } from "react";
import { useIntl } from "react-intl"

export default function InvitationsButton() {

    const intl = useIntl();
    const router = useRouter();
    const [progress, setProgress] = useState(true);
    const [count, setCount] = useState<number | null>(null);
    const load = useCallback(async () => {

        setProgress(true);

        const { data, errors } = await rpc('workspace/user-invitations/countAll', {})

        setCount(errors ? null : data.count)

        setProgress(false);

    }, []);

    useEffect(() => {
        load();
    }, [load])

    return <Button variant="outline" onClick={() => router.push('/workspaces/invitations')} disabled={count === 0 || progress}>
        {progress ? <SpinnerIcon className="animate-spin" /> : <SendIcon />}
        <span className="hidden sm:flex">
            {intl.formatMessage({ id: 'workspaces.members.menu.invitations' })}
        </span>
        {count && count > 0 ? <Badge variant="default" className="ml-2">{count}</Badge> : ''}
    </Button>

}