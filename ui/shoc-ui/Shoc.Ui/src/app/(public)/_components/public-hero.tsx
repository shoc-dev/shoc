"use client"

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { ArrowUpRight, LayoutTemplate, SquareChevronRight } from "lucide-react";
import Link from "next/link";
import React from "react";
import { useIntl } from "react-intl";

export default function PublicHero() {
    const intl = useIntl();
    return (
        <div className="min-h-[calc(100vh-6rem)] flex flex-col items-center py-20 px-6">
            <div className="md:mt-6 flex items-center justify-center">
                <div className="text-center max-w-2xl">
                    <Badge className="bg-primary rounded-full py-1 border-none">
                        {intl.formatMessage({ id: 'landing.hero.betaLabel' })}
                    </Badge>
                    <h1 className="mt-6 max-w-[20ch] text-3xl xs:text-4xl sm:text-5xl md:text-6xl font-bold !leading-[1.2] tracking-tight">
                        {intl.formatMessage({ id: 'landing.hero.title' })}
                    </h1>
                    <p className="mt-6 max-w-[70ch] xs:text-lg">
                        {intl.formatMessage({ id: 'landing.hero.description' })}
                    </p>
                    <div className="mt-12 flex flex-col sm:flex-row items-center sm:justify-center gap-4">
                        <Link prefetch={false} href="/workspaces">
                            <Button
                                size="lg"
                                className="w-full sm:w-auto rounded-full text-base"
                            >
                                {intl.formatMessage({ id: 'landing.getStarted' })} <ArrowUpRight className="!h-5 !w-5" />
                            </Button>
                        </Link>
                        <Link prefetch={false} href="/templates">
                            <Button
                                variant="outline"
                                size="lg"
                                className="w-full sm:w-auto rounded-full text-base shadow-none"
                            >
                                <LayoutTemplate className="!h-5 !w-5" /> {intl.formatMessage({ id: 'landing.navbar.templates' })}
                            </Button>
                        </Link>
                    </div>
                    <div className="flex items-center justify-center mt-6 rounded-md font-medium bg-zinc-900 px-4 py-3 text-center font-mono text-sm text-zinc-100">
                        <SquareChevronRight className="w-4 h-4 mr-3" />
                        <span>npm i -g @shoc/shoc</span>
                    </div>
                </div>
            </div>
        </div>
    );
};