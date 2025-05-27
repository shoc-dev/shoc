"use client"

import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuLabel,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { MoreHorizontal } from "lucide-react"
import { useIntl } from "react-intl"
import { Button } from "@/components/ui/button"
import Link from "next/link"
import JobStatusBadge from "./job-status-badge"
import { durationBetween } from "@/extended/format"
import { Badge } from "@/components/ui/badge"
import { JobValueType } from "@/domain/job"
import { useRouter } from "next/navigation"
import { cn } from "@/lib/utils"

export default function JobsTable({ items, workspaceName, className }: { items: any, workspaceName: string, className?: string }) {

    const intl = useIntl();
    const router = useRouter();

    return <table className={cn("table-auto caption-bottom text-sm", className)} data-slot="table">
        <TableHeader>
            <TableRow>
                <TableHead className="w-6">#</TableHead>
                <TableHead className="w-[200px]">{intl.formatMessage({ id: 'global.labels.name' })}</TableHead>
                <TableHead className="w-[200px]">{intl.formatMessage({ id: 'global.labels.description' })}</TableHead>
                <TableHead className="w-[70px]">{intl.formatMessage({ id: 'jobs.labels.tasks' })}</TableHead>
                <TableHead className="w-[180px]">{intl.formatMessage({ id: 'global.labels.status' })}</TableHead>
                <TableHead className="w-[180px]">{intl.formatMessage({ id: 'jobs.labels.scope' })}</TableHead>
                <TableHead className="w-[300px]">{intl.formatMessage({ id: 'jobs.labels.cluster' })}</TableHead>
                <TableHead className="w-[300px]">{intl.formatMessage({ id: 'jobs.labels.actor' })}</TableHead>
                <TableHead className="w-[150px]">{intl.formatMessage({ id: 'jobs.labels.waiting' })}</TableHead>
                <TableHead className="w-[150px]">{intl.formatMessage({ id: 'jobs.labels.running' })}</TableHead>
                <TableHead className="text-right">{intl.formatMessage({ id: 'global.labels.actions' })}</TableHead>
            </TableRow>
        </TableHeader>
        <TableBody>
            {items.map((item: JobValueType) => (
                <TableRow key={item.id}>
                    <TableCell>
                        <Link prefetch={false} className="underline" href={`/workspaces/${workspaceName}/jobs/${item.localId}`}>
                            {item.localId}
                        </Link>
                    </TableCell>
                    <TableCell className="font-medium">
                        <Link prefetch={false} className="underline" href={`/workspaces/${workspaceName}/jobs/${item.localId}`}>
                            {item.name ? <span className="overflow-hidden truncate" title={item.name}>
                                {item.name}
                            </span> :
                                <span className="italic">
                                    {intl.formatMessage({ id: 'jobs.labels.untitled' })}
                                </span>
                            }
                        </Link>
                    </TableCell>
                    <TableCell className="font-medium">
                        {item.description ? <span className="overflow-hidden truncate" title={item.description}>
                            {item.description}
                        </span> :
                            <span className="italic">
                                {intl.formatMessage({ id: 'jobs.labels.noDesc' })}
                            </span>
                        }
                    </TableCell>
                    <TableCell className="font-medium">
                        {item.completedTasks} / {item.totalTasks}
                    </TableCell>
                    <TableCell className="font-medium">
                        <JobStatusBadge status={item.status} />
                    </TableCell>
                    <TableCell className="font-medium">
                        <Badge variant="outline">{intl.formatMessage({ id: `jobs.scopes.${item.scope}` })}</Badge>
                    </TableCell>
                    <TableCell className="font-medium">
                        <Link prefetch={false} className="underline" target="_blank" href={`/workspaces/${workspaceName}/clusters/${item.clusterName}`}>
                            {item.clusterName}
                        </Link>
                    </TableCell>
                    <TableCell className="font-medium">
                        <span>
                            {item.userFullName}
                        </span>
                    </TableCell>
                    <TableCell className="font-medium">
                        {durationBetween(item.created, item.runningAt)}
                    </TableCell>
                    <TableCell className="font-medium">
                        {item.runningAt ? durationBetween(item.runningAt, item.completedAt) : "N/A"}
                    </TableCell>
                    <TableCell className="text-right">
                        <DropdownMenu modal={false}>
                            <DropdownMenuTrigger asChild>
                                <Button variant="ghost" size="sm" className="h-8 w-8 p-0">
                                    <span className="sr-only">Open menu</span>
                                    <MoreHorizontal className="h-4 w-4" />
                                </Button>
                            </DropdownMenuTrigger>
                            <DropdownMenuContent align="end">
                                <DropdownMenuLabel>{intl.formatMessage({ id: 'global.labels.actions' })}</DropdownMenuLabel>
                                <DropdownMenuItem onClick={() => router.push(`/workspaces/${workspaceName}/jobs/${item.localId}`)}>
                                    {intl.formatMessage({ id: 'global.actions.more' })}
                                </DropdownMenuItem>
                            </DropdownMenuContent>
                        </DropdownMenu>
                    </TableCell>
                </TableRow>
            ))}
        </TableBody>
    </table>
}