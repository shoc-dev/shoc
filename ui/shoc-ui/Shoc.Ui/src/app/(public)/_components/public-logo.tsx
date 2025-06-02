"use client"

import Image from "next/image";
import Link from "next/link";
import { useIntl } from "react-intl";

export default function PublicLogo(){
    const intl = useIntl();

    return <Link prefetch={false} href="/" className="flex items-center gap-2.5">
    <Image src="/logos/shoc-512x512.png" className="w-6 h-6" width={128} height={128} alt={intl.formatMessage({id: 'shoc.platform'})} />
    <h2 className="text-md font-bold font-code inline">{intl.formatMessage({id: 'shoc.platform'})}</h2>
  </Link>
}