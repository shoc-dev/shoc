"use client"

import * as React from "react"
import {
  ColumnDef,
  ColumnFiltersState,
  PaginationState,
  SortingState,
  Table as TableType,
  VisibilityState,
  flexRender,
  getCoreRowModel,
  getFacetedRowModel,
  getFacetedUniqueValues,
  getFilteredRowModel,
  getPaginationRowModel,
  getSortedRowModel,
  useReactTable,
} from "@tanstack/react-table"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import DataTablePagination from "./data-table-pagination"
import { cn } from "@/lib/utils"
import LoadingContainer from "../general/loading-container"
import ErrorScreen from "../error/error-screen"

interface DataTableProps<TData, TValue> {
  columns: ColumnDef<TData, TValue>[]
  data: TData[],
  className?: string,
  progress?: boolean,
  errors?: any[],
  defaultPagination?: PaginationState,
  toolbar?: (table: TableType<TData>) => React.ReactNode
}

const DEFAULT_PAGINATION = {
  pageIndex: 0,
  pageSize: 20
}

export default function DataTable<TData, TValue>({
  columns,
  data,
  className,
  progress = false,
  errors = [],
  defaultPagination = DEFAULT_PAGINATION,
  toolbar
}: DataTableProps<TData, TValue>) {
  const [sorting, setSorting] = React.useState<SortingState>([])
  const [columnFilters, setColumnFilters] = React.useState<ColumnFiltersState>(
    []
  )
  const [columnVisibility, setColumnVisibility] =
    React.useState<VisibilityState>({})
  const [rowSelection, setRowSelection] = React.useState({})
  const [pagination, setPagination] = React.useState<PaginationState>({...DEFAULT_PAGINATION, ...defaultPagination})

  const hasErrors = errors && errors.length > 0;

  const table = useReactTable({
    data,
    columns,
    state: {
      sorting,
      columnVisibility,
      rowSelection,
      columnFilters,
      pagination: pagination
    },
    enableRowSelection: true,
    onRowSelectionChange: setRowSelection,
    onSortingChange: setSorting,
    onColumnFiltersChange: setColumnFilters,
    onColumnVisibilityChange: setColumnVisibility,
    onPaginationChange: setPagination,
    getCoreRowModel: getCoreRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getFacetedRowModel: getFacetedRowModel(),
    getFacetedUniqueValues: getFacetedUniqueValues(),
  })

  return (
    <LoadingContainer loading={progress}>

      <div className={cn("space-y-4", className)}>
        {toolbar && toolbar(table)}
        <div className={cn("rounded-md border")}>
          <Table>
            <TableHeader>
              {table.getHeaderGroups().map((headerGroup) => (
                <TableRow key={headerGroup.id}>
                  {headerGroup.headers.map((header) => {
                    return (
                      <TableHead className={cn(progress || hasErrors ? "pointer-events-none" : "")} key={header.id}>
                        {header.isPlaceholder
                          ? null
                          : flexRender(
                            header.column.columnDef.header,
                            header.getContext()
                          )}
                      </TableHead>
                    )
                  })}
                </TableRow>
              ))}
            </TableHeader>
            <TableBody>
              {!hasErrors && table.getRowModel().rows?.length > 0 && (
                table.getRowModel().rows.map((row) => (
                  <TableRow
                    key={row.id}
                    data-state={row.getIsSelected() && "selected"}
                  >
                    {row.getVisibleCells().map((cell) => (
                      <TableCell key={cell.id}>
                        {flexRender(
                          cell.column.columnDef.cell,
                          cell.getContext()
                        )}
                      </TableCell>
                    ))}
                  </TableRow>
                ))
              )}
              {!hasErrors && !table.getRowModel().rows?.length && (
                <TableRow>
                  <TableCell
                    colSpan={columns.length}
                    className="h-24 text-center"
                  >
                    No results.
                  </TableCell>
                </TableRow>

              )}
              {hasErrors && (
                <TableRow>
                  <TableCell
                    colSpan={columns.length}
                    className="h-[500px] bg-white text-center"
                  >
                    <ErrorScreen errors={errors} />
                  </TableCell>
                </TableRow>

              )}
            </TableBody>
          </Table>
        </div>
        {!hasErrors && <DataTablePagination table={table} />}
      </div>
    </LoadingContainer>
  )
}