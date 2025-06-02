"use client"
import { IntlMessageId } from "@/i18n/sources";
import ClusterIcon from "@/components/icons/cluster-icon";
import {
    Boxes,
    KeyRoundIcon,
    Layers,
    Play,
    ShieldCheck,
} from "lucide-react";
import React, { FunctionComponent } from "react";
import { useIntl } from "react-intl";

const features: { icon: FunctionComponent<{ className?: string }>, title: IntlMessageId, description: IntlMessageId }[] = [
    {
        icon: Layers,
        title: 'landing.features.containerization.title',
        description: 'landing.features.containerization.description',
    },
    {
        icon: Play,
        title: 'landing.features.scheduling.title',
        description: 'landing.features.scheduling.description'
    },
    {
        icon: ClusterIcon,
        title: 'landing.features.clustering.title',
        description: 'landing.features.clustering.description',
    },
    {
        icon: KeyRoundIcon,
        title: 'landing.features.secrets.title',
        description: 'landing.features.secrets.description',
    },
    {
        icon: Boxes,
        title: 'landing.features.registry.title',
        description: 'landing.features.registry.description',
    },
    {
        icon: ShieldCheck,
        title: 'landing.features.security.title',
        description: 'landing.features.security.description'
    }
];

export default function PublicFeatures() {
    const intl = useIntl();
    return (
        <div id="features" className="w-full py-12 xs:py-20 px-6">
            <h2 className="text-3xl xs:text-4xl sm:text-5xl font-bold tracking-tight text-center">
                {intl.formatMessage({id: 'landing.features.title'})}
            </h2>
            <div className="w-full max-w-screen-lg mx-auto mt-10 sm:mt-16 grid sm:grid-cols-2 lg:grid-cols-3 gap-6">
                {features.map((feature, index) => (
                    <div
                        key={`ftr-${index}`}
                        className="flex flex-col bg-background border rounded-xl py-6 px-5"
                    >
                        <div className="mb-3 h-10 w-10 flex items-center justify-center bg-muted rounded-full">
                            <feature.icon className="h-6 w-6" />
                        </div>
                        <span className="text-lg font-semibold">{intl.formatMessage({ id: feature.title })}</span>
                        <p className="mt-1 text-foreground/80 text-[15px]">
                            {intl.formatMessage({ id: feature.description })}
                        </p>
                    </div>
                ))}
            </div>
        </div>
    );
};