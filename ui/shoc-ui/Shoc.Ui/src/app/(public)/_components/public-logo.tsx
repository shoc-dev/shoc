"use client"

import { CommandIcon } from "lucide-react";
import Link from "next/link";
import { useIntl } from "react-intl";

export default function PublicLogo(){
    const intl = useIntl();

    return <Link prefetch={false} href="/" className="flex items-center gap-2.5">
    <CommandIcon className="w-6 h-6 text-muted-foreground" strokeWidth={2} />
    <h2 className="text-md font-bold font-code inline">{intl.formatMessage({id: 'shoc.platform'})}</h2>
  </Link>
}