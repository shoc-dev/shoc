"use client"
import useWorkspace from "@/providers/workspace/use-workspace";
import { Breadcrumb, BreadcrumbItem, BreadcrumbLink, BreadcrumbList, BreadcrumbPage, BreadcrumbSeparator } from "../ui/breadcrumb";
import React, { ReactNode } from "react";
import { useIntl } from "react-intl";

export default function WorkspacePageBreadcrumbs({ className, hideHome, crumbs, title, titleAddon }: { className?: string, hideHome?: boolean, crumbs?: ReactNode[], title: string | ReactNode, titleAddon?: ReactNode }) {

    const intl = useIntl();
    const { value: workspace } = useWorkspace()

    return <Breadcrumb className={className}>
        <BreadcrumbList>
            {!hideHome && <BreadcrumbItem key="workspaces" className="hidden md:block">
                <BreadcrumbLink href={`/workspaces`}>
                    {intl.formatMessage({ id: 'workspaces' })}
                </BreadcrumbLink>
            </BreadcrumbItem>
            }
            {!hideHome && <BreadcrumbSeparator key="spr-root" className="hidden md:block" />}
            {!hideHome && <BreadcrumbItem key="bc-home" className="hidden md:block">
                <BreadcrumbLink href={`/workspaces/${workspace.name}`}>
                    {workspace.name}
                </BreadcrumbLink>
            </BreadcrumbItem>
            }
            {!hideHome && <BreadcrumbSeparator key="spr-home" className="hidden md:block" />}
            {crumbs?.map((crumb, index) => <React.Fragment key={`crmb-${index}`}>
                <BreadcrumbItem key={`bcl-${index}`} className="hidden md:block">
                    {crumb}
                </BreadcrumbItem>
                <BreadcrumbSeparator key={`spr-${index}`} className="hidden md:block" />
            </React.Fragment>
            )}
            <BreadcrumbItem key="bc-title">
                <BreadcrumbPage>{title}</BreadcrumbPage>
            </BreadcrumbItem>
            {titleAddon}
        </BreadcrumbList>
    </Breadcrumb>
}