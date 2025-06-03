"use client"

import {
    NavigationMenu, NavigationMenuItem, NavigationMenuLink, NavigationMenuList,
} from "@/components/ui/navigation-menu";
import { NavigationMenuProps } from "@radix-ui/react-navigation-menu";
import Link from "next/link";
import { useIntl } from "react-intl";

export default function PublicNavMenu(props: NavigationMenuProps) {
    const intl = useIntl();

    return (
        <NavigationMenu {...props}>
            <NavigationMenuList className="data-[orientation=vertical]:gap-2 space-x-0 data-[orientation=vertical]:flex-col data-[orientation=vertical]:items-start">
                <NavigationMenuItem>
                    <NavigationMenuLink asChild>
                        <Link prefetch={false} href="/#features">{intl.formatMessage({ id: 'landing.navbar.menu.features' })}</Link>
                    </NavigationMenuLink>
                </NavigationMenuItem>
                <NavigationMenuItem>
                    <NavigationMenuLink asChild>
                        <Link prefetch={false} href="/templates">{intl.formatMessage({ id: 'landing.navbar.templates' })}</Link>
                    </NavigationMenuLink>
                </NavigationMenuItem>
            </NavigationMenuList>
        </NavigationMenu>
    )
};