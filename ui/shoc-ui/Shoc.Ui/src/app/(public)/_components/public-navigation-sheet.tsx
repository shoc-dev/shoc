"use client"
import { Button } from "@/components/ui/button";
import { Sheet, SheetContent, SheetDescription, SheetHeader, SheetTitle, SheetTrigger } from "@/components/ui/sheet";
import { Menu } from "lucide-react";
import PublicNavMenu from "./public-nav-menu";
import PublicLogo from "./public-logo";
import { useIntl } from "react-intl";
import Link from "next/link";
import PublicSignInButton from "./public-sign-in-button";

export default function PublicNavigationSheet() {
    const intl = useIntl();
    return (
        <Sheet>
            <SheetTrigger asChild>
                <Button variant="outline" size="icon" className="rounded-full">
                    <Menu />
                </Button>
            </SheetTrigger>
            <SheetContent>
                <SheetHeader>
                    <SheetTitle>
                        <PublicLogo />
                    </SheetTitle>
                    <SheetDescription className="hidden">
                    </SheetDescription>
                    <PublicNavMenu orientation="vertical" className="mt-6" />
                </SheetHeader>
                <div className="mt-8 space-y-4">
                    <PublicSignInButton className="w-full rounded-lg" />
                    <Link prefetch={false} href="/workspaces">
                        <Button className="w-full xs:hidden rounded-lg">
                            {intl.formatMessage({ id: 'landing.getStarted' })}
                        </Button>
                    </Link>
                </div>
            </SheetContent>
        </Sheet>
    );
};
