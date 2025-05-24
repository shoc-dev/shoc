"use client"

import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuLabel,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { MoreHorizontal, Play } from "lucide-react"
import { useIntl } from "react-intl"
import { Job } from "./types"
import { Button } from "@/components/ui/button"
import Link from "next/link"
import JobStatusBadge from "./job-status-badge"
import { durationBetween } from "@/extended/format"
import { Badge } from "@/components/ui/badge"

export default function JobsTable({ items, workspaceName, className }: { items: any, workspaceName: string, className?: string }) {

    const intl = useIntl();

    return <Table className={`${className} table-auto`}>
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
            {items.map((item: Job) => (
                <TableRow key={item.id}>
                    <TableCell>
                        <Link prefetch={false} href={`/workspaces/${workspaceName}/jobs/${item.localId}`}>
                            {item.localId}
                        </Link>
                    </TableCell>
                    <TableCell className="font-medium">
                        <Link prefetch={false} href={`/workspaces/${workspaceName}/jobs/${item.localId}`}>
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
                                <DropdownMenuLabel>Actions</DropdownMenuLabel>
                                <DropdownMenuItem>View details</DropdownMenuItem>
                                <DropdownMenuItem>View logs</DropdownMenuItem>
                                <DropdownMenuSeparator />
                                <DropdownMenuItem>
                                    <Play className="mr-2 h-4 w-4" />
                                    Re-run workflow
                                </DropdownMenuItem>
                                <DropdownMenuItem>Download artifacts</DropdownMenuItem>
                            </DropdownMenuContent>
                        </DropdownMenu>
                    </TableCell>
                </TableRow>
            ))}
        </TableBody>
    </Table>
}