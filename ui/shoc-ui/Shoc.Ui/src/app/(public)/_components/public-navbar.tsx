"use client"

import { Button } from "@/components/ui/button";
import PublicNavigationSheet from "./public-navigation-sheet";
import PublicNavMenu from "./public-nav-menu";
import PublicLogo from "./public-logo";
import PublicSignInButton from "./public-sign-in-button";
import Link from "next/link";
import { useIntl } from "react-intl";

export default function PublicNavbar() {
  const intl = useIntl();

  return (
    <nav className="fixed z-10 top-6 inset-x-4 h-14 xs:h-16 bg-background/50 backdrop-blur-sm border dark:border-slate-700/70 max-w-screen-xl mx-auto rounded-full">
      <div className="h-full flex items-center justify-between mx-auto px-4">
        <PublicLogo />

        <PublicNavMenu className="hidden md:block" />

        <div className="flex items-center gap-3">
          <Link prefetch={false} href="/workspaces">
            <Button className="hidden sm:inline-flex rounded-full">
              {intl.formatMessage({ id: 'landing.getStarted' })}
            </Button>
          </Link>
          <PublicSignInButton className="hidden sm:inline-flex" />
          
          <div className="md:hidden">
            <PublicNavigationSheet />
          </div>
        </div>
      </div>
    </nav>
  );
};