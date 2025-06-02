"use client"

import Link from "next/link";
import { PublicFooterButtons } from "./public-footer-buttons";
import { useIntl } from "react-intl";
import staticLinks from "@/well-known/static-links";
import Image from "next/image";

export default function PublicFooter() {
    const intl = useIntl();

    return (
        <footer className="border-t w-full h-16">
            <div className="container mx-auto flex items-center sm:justify-between justify-center sm:gap-0 gap-4 h-full text-muted-foreground text-sm flex-wrap sm:py-0 py-3 max-sm:px-4">
                <div className="flex items-center gap-3">
                    <Image src="/logos/shoc-512x512.png" className="sm:block hidden w-5 h-5" width={128} height={128} alt={intl.formatMessage({ id: 'shoc.platform' })} />
                    <p className="text-center">
                        {intl.formatMessage({id: 'global.builtBy'})} {" "}
                        <Link
                            prefetch={false}
                            className="px-1 underline underline-offset-2"
                            href={staticLinks.shocAuthorRepo}
                            target="_blank"
                        >
                            davitp
                        </Link>
                        | {intl.formatMessage({id: 'global.footer.sourceNotice'})}{" "}
                        <Link
                            prefetch={false}
                            className="px-1 underline underline-offset-2"
                            href={staticLinks.githubRepo}
                            target="_blank"
                        >
                            GitHub
                        </Link>
                        
                    </p>
                </div>

                <div className="gap-4 items-center hidden md:flex">
                    <PublicFooterButtons />
                </div>
            </div>
        </footer>
    );
}
