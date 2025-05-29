"use client"

import DataTable from "@/components/data-table"
import { type ColumnDef } from "@tanstack/react-table"
import { Badge } from "@/components/ui/badge"
import DataTableColumnHeader from "@/components/data-table/data-table-column-header"
import { useIntl } from "react-intl"
import KeyIcon from "@/components/icons/key-icon"
import { workspaceRolesMap } from "@/well-known/workspace"
import { localDate } from "@/extended/format"
import { MoreHorizontal, PlaneIcon, SendIcon } from "lucide-react"
import { Button } from "@/components/ui/button"
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuLabel, DropdownMenuTrigger } from "@/components/ui/dropdown-menu"
import { rpc } from "@/server-actions/rpc"
import { useCallback, useEffect, useMemo, useState } from "react"
import useWorkspaceAccess from "@/providers/workspace-access/use-workspace-access"
import WorkspaceInvitationDeleteDialog from "./workspace-invitation-delete-dialog"
import WorkspaceInvitationUpdateDialog from "./workspace-invitation-update-dialog"
import WorkspaceInvitationCreateDialog from "./workspace-invitation-create-dialog"
import DataTableToolbar from "@/components/data-table/data-table-toolbar"
import { WorkspacePermissions } from "@/well-known/workspace-permissions"

export default function WorkspaceInvitationsTable({ workspaceId, className }: { workspaceId: string, className?: string }) {

  const intl = useIntl();
  const { hasAny } = useWorkspaceAccess();
  const [progress, setProgress] = useState(true);
  const [data, setData] = useState<any[]>([]);
  const [errors, setErrors] = useState<any[]>([]);
  const [creatingActive, setCreatingActive] = useState<any>(false);
  const [editingItem, setEditingItem] = useState<any>(null);
  const [deletingItem, setDeletingItem] = useState<any>(null);

  const load = useCallback(async (workspaceId: string) => {

    setProgress(true);
    const { data, errors } = await rpc('workspace/user-workspace-invitations/getAll', { workspaceId })

    if (errors) {
      setErrors(errors);
      setData([]);
    } else {
      setErrors([]);
      setData(data)
    }

    setProgress(false);

  }, []);


  useEffect(() => {
    if (!workspaceId) {
      return;
    }
    load(workspaceId);
  }, [workspaceId, load])

  const columns: ColumnDef<any>[] = useMemo(() => [
    {
      accessorKey: "email",
      header: ({ column }) => (
        <DataTableColumnHeader column={column} title={intl.formatMessage({ id: 'global.labels.email' })} />
      ),
      cell: ({ row }) => <div>{row.getValue("email")}</div>,
      enableSorting: true,
      enableHiding: false,
    },
    {
      accessorKey: "role",
      header: ({ column }) => (
        <DataTableColumnHeader column={column} title={intl.formatMessage({ id: 'workspaces.labels.role' })} />
      ),
      cell: ({ row }) => <Badge variant="secondary">
        <KeyIcon className="w-4 h-4 mr-1" />
        {intl.formatMessage({ id: 'workspaces.labels.role' })}: {intl.formatMessage({ id: workspaceRolesMap[row.getValue('role') as string] })}
      </Badge>,
      enableSorting: true,
      enableHiding: false,
    },
    {
      accessorKey: "invitedByFullName",
      header: ({ column }) => (
        <DataTableColumnHeader column={column} title={intl.formatMessage({ id: 'workspaces.invitations.labels.invitedBy' })} />
      ),
      cell: ({ row }) => <span>{row.getValue("invitedByFullName")}</span>,
      enableSorting: true,
      enableHiding: false,
      size: 100
    },
    {
      accessorKey: "expiration",
      header: ({ column }) => (
        <DataTableColumnHeader column={column} title={intl.formatMessage({ id: 'global.labels.expiration' })} />
      ),
      cell: ({ row }) => localDate(row.getValue('expiration')),
      enableSorting: true,
      enableHiding: false,
    },
    {
      accessorKey: "created",
      header: ({ column }) => (
        <DataTableColumnHeader column={column} title={intl.formatMessage({ id: 'global.labels.created' })} />
      ),
      cell: ({ row }) => localDate(row.getValue('created')),
      enableSorting: true,
      enableHiding: false,
    },
    {
      accessorKey: "updated",
      header: ({ column }) => (
        <DataTableColumnHeader column={column} title={intl.formatMessage({ id: 'global.labels.updated' })} />
      ),
      cell: ({ row }) => localDate(row.getValue('updated')),
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
              <DropdownMenuItem
                onClick={() => setEditingItem(row.original)}
                disabled={row.original.role === 'owner' || !hasAny([WorkspacePermissions.WORKSPACE_UPDATE_INVITATION])}
              >
                {intl.formatMessage({ 'id': 'global.actions.update' })}
              </DropdownMenuItem>
              <DropdownMenuItem
                className="text-red-600 hover:text-red-600! hover:bg-red-100!"
                onClick={() => setDeletingItem(row.original)}
                disabled={row.original.role === 'owner' || !hasAny([WorkspacePermissions.WORKSPACE_DELETE_INVITATION])}
              >
                {intl.formatMessage({ 'id': 'global.actions.delete' })}
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </>
        )
      }
    }
  ], [intl, hasAny])

  return (
    <>
      <WorkspaceInvitationCreateDialog
        workspaceId={workspaceId}
        open={creatingActive}
        onClose={() => setCreatingActive(false)} onSuccess={() => {
          load(workspaceId)
        }} />
      <WorkspaceInvitationUpdateDialog
        workspaceId={workspaceId}
        item={editingItem}
        open={editingItem}
        onClose={() => setEditingItem(null)} onSuccess={() => {
          load(workspaceId)
        }} />
      <WorkspaceInvitationDeleteDialog
        open={deletingItem}
        onClose={() => setDeletingItem(null)}
        item={deletingItem}
        onSuccess={() => {
          load(workspaceId)
        }}
      />

      <DataTable
        className={className}
        data={data}
        columns={columns}
        progress={progress}
        errors={errors}
        toolbar={(table) => <DataTableToolbar className="flex flex-col items-start" table={table}>
          <Button variant="outline" onClick={() => setCreatingActive(true)}>
            <SendIcon className="mr-2 h-4 w-4" /> {intl.formatMessage({id: 'workspaces.invitations.create.action'})} 
          </Button>
        </DataTableToolbar> }
      />
    </>
  )
}