"use client"

import { buttonVariants } from "@/components/ui/button";
import staticLinks from "@/well-known/static-links";
import { HeartIcon } from "lucide-react";
import Link from "next/link";
import { useIntl } from "react-intl";

export function PublicFooterButtons() {
    const intl = useIntl();
    return (
      <>
        
        <Link
          prefetch={false}
          target="_blank"
          href={staticLinks.githubRepo}
          className={buttonVariants({ variant: "outline", size: "sm" })}
        >
          <HeartIcon className="h-4 w-4 mr-2 text-red-600 fill-current" />
          {intl.formatMessage({id: 'global.sponsor'})}
        </Link>
      </>
    );
  }