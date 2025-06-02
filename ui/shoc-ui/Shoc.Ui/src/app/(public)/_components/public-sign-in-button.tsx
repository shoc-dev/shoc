"use client"
import { signIn } from "@/addons/auth";
import SpinnerIcon from "@/components/icons/spinner-icon";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { rpc, rpcDirect } from "@/server-actions/rpc";
import { useSession } from "next-auth/react";
import { useIntl } from "react-intl";

export default function PublicSignInButton({ className }: { className?: string }) {

    const session = useSession();
    const intl = useIntl();
    const authenticated = session?.status === 'authenticated';


    return <Button variant="outline" disabled={session?.status === 'loading'} className={cn("rounded-full", className)} onClick={async () => {

        if (!authenticated) {
            await rpcDirect('auth/signIn', {})
        }

        if (authenticated) {
            const { errors, data } = await rpc('auth/signleSignOut', { postLogoutRedirectUri: `${window.location.origin}/signed-out` })
            if (errors || !data) {
                return;
            }
            document.location.href = data.redirectUri
        }

    }}>
        {session?.status === 'loading' && (
            <SpinnerIcon className="h-4 w-4 animate-spin" />
        )}
        {!authenticated && intl.formatMessage({ id: 'login' })}
        {authenticated && intl.formatMessage({ id: 'logout' })}
    </Button>

}