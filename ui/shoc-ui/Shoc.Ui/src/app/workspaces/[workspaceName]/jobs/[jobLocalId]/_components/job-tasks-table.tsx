"use client"

import DataTable from "@/components/data-table"
import { type ColumnDef } from "@tanstack/react-table"
import DataTableColumnHeader from "@/components/data-table/data-table-column-header"
import { useIntl } from "react-intl"
import { localDate } from "@/extended/format"
import { MoreHorizontal } from "lucide-react"
import { Button } from "@/components/ui/button"
import { DropdownMenu, DropdownMenuContent, DropdownMenuLabel, DropdownMenuTrigger } from "@/components/ui/dropdown-menu"
import { useMemo } from "react"
import { JobTaskValueType } from "@/domain/job"
import useJobTasks from "../_providers/job-tasks/use-job-tasks"
import Link from "next/link"
import JobTaskStatusBadge from "./job-task-status-badge"

export default function JobTasksTable({ className }: { className?: string }) {

  const intl = useIntl();
  const { value: tasks, loading, errors } = useJobTasks()

  const columns: ColumnDef<JobTaskValueType>[] = useMemo(() => [
    {
      accessorKey: "sequence",
      header: ({ column }) => (
        <DataTableColumnHeader column={column} title="#" />
      ),
      cell: ({ row }) => <div>{row.getValue("sequence")}</div>,
      enableSorting: true,
      enableHiding: false,
    },
    {
      accessorKey: "status",
      header: ({ column }) => (
        <DataTableColumnHeader column={column} title={intl.formatMessage({id: 'global.labels.status'})} />
      ),
      cell: ({ row }) => <JobTaskStatusBadge status={row.original.status} />,
      enableSorting: true,
      enableHiding: false,
    },
    {
      accessorKey: "clusterName",
      header: ({ column }) => (
        <DataTableColumnHeader column={column} title={intl.formatMessage({ id: 'jobs.labels.cluster' })} />
      ),
      cell: ({ row }) => <Link className="underline" prefetch={false} href={`/workspaces/${row.original.workspaceName}/clusters/${row.original.clusterName}`} target="_blank">
        {row.original.clusterName}
      </Link>,
      enableSorting: true,
      enableHiding: false,
    },
    {
      accessorKey: "actor",
      header: ({ column }) => (
        <DataTableColumnHeader column={column} title={intl.formatMessage({ id: 'jobs.labels.actor' })} />
      ),
      cell: ({ row }) => row.original.userFullName,
      enableSorting: true,
      enableHiding: false,
    },
    {
      id: "actions",
      enableHiding: false,
      enableSorting: false,
      header: ({ column }) => (
        <DataTableColumnHeader column={column} title={intl.formatMessage({ id: 'global.labels.actions' })} />
      ),
      cell: function Cell({ row }) {
        return (<>
          <DropdownMenu modal={false}>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" className="h-8 w-8 p-0">
                <MoreHorizontal className="h-4 w-4" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuLabel>{intl.formatMessage({ id: 'global.labels.actions' })}</DropdownMenuLabel>
            </DropdownMenuContent>
          </DropdownMenu>
        </>
        )
      }
    }
  ], [intl])

  const data = useMemo(() => tasks ?? [], [tasks]);

  return (
    <>
      <DataTable
        className={className}
        data={data}
        columns={columns}
        progress={loading}
        errors={errors}
      />
    </>
  )
}