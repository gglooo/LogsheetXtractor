import { Button } from "@/components/ui/button";
import {
    Pagination,
    PaginationContent,
    PaginationItem,
} from "@/components/ui/pagination";
import { Skeleton } from "@/components/ui/skeleton";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table";
import { useLogsheets } from "@/modules/logsheets/api";
import { PreviewModal } from "@/modules/logsheets/components/preview-modal";
import { LogsheetTableBulkActions } from "@/modules/logsheets/table/bulk-actions";
import { useLogsheetsColumns } from "@/modules/logsheets/table/columns";
import {
    flexRender,
    getCoreRowModel,
    getFilteredRowModel,
    getPaginationRowModel,
    getSortedRowModel,
    useReactTable,
    type PaginationState,
    type RowSelectionState,
} from "@tanstack/react-table";
import { ChevronLeftIcon, ChevronRightIcon } from "lucide-react";
import { useState, type ComponentProps } from "react";
import { useIntl } from "react-intl";

export function TableSkeleton(props: ComponentProps<typeof Table>) {
    const columns = useLogsheetsColumns();
    return (
        <Table {...props}>
            <TableHeader>
                <TableRow>
                    {columns.map((_, index) => (
                        <TableHead key={index}>
                            <Skeleton className="h-4 w-25" />
                        </TableHead>
                    ))}
                </TableRow>
            </TableHeader>
            <TableBody>
                {Array.from({ length: 5 }).map((_, rowIndex) => (
                    <TableRow key={rowIndex}>
                        {columns.map((_, colIndex) => (
                            <TableCell key={colIndex}>
                                <Skeleton
                                    className={`h-6 ${
                                        colIndex % 3 === 0
                                            ? "w-[60%]"
                                            : colIndex % 3 === 1
                                              ? "w-[80%]"
                                              : "w-[40%]"
                                    }`}
                                />
                            </TableCell>
                        ))}
                    </TableRow>
                ))}
            </TableBody>
        </Table>
    );
}

export const LogsheetTable = ({
    templateId,
    ...props
}: { templateId: string } & ComponentProps<typeof Table>) => {
    const intl = useIntl();

    const logsheetsQuery = useLogsheets(templateId);
    const [previewLogsheetId, setPreviewLogsheetId] = useState<string | null>(
        null,
    );

    const columns = useLogsheetsColumns();
    const [pagination, setPagination] = useState<PaginationState>({
        pageIndex: 0,
        pageSize: 10,
    });
    const [selectedRowIds, setSelectedRowIds] = useState<RowSelectionState>({});

    // eslint-disable-next-line react-hooks/incompatible-library
    const table = useReactTable({
        data: logsheetsQuery.data ?? [],
        columns: columns,
        getCoreRowModel: getCoreRowModel(),
        getSortedRowModel: getSortedRowModel(),
        getFilteredRowModel: getFilteredRowModel(),
        getPaginationRowModel: getPaginationRowModel(),
        onPaginationChange: setPagination,
        onRowSelectionChange: setSelectedRowIds,
        getRowId: (originalRow) => originalRow.id,
        state: {
            pagination,
            rowSelection: selectedRowIds,
        },
        meta: {
            onPreview: (id: string) => setPreviewLogsheetId(id),
        },
    });

    if (logsheetsQuery.isLoading) {
        return <TableSkeleton {...props} />;
    }

    if (logsheetsQuery.isError) {
        return (
            <div className="p-4">
                {intl.formatMessage({
                    id: "logsheets.table.load-error",
                    defaultMessage: "Failed to load logsheets.",
                })}
            </div>
        );
    }

    return (
        <div className="flex flex-col gap-4">
            <LogsheetTableBulkActions
                selectedLogsheetIds={Object.keys(selectedRowIds)}
                onClearSelection={() => setSelectedRowIds({})}
            />
            <Table {...props}>
                <TableHeader>
                    {table.getHeaderGroups().map((headerGroup) => (
                        <TableRow key={headerGroup.id}>
                            {headerGroup.headers.map((header) => (
                                <TableHead
                                    key={header.id}
                                    colSpan={header.colSpan}
                                >
                                    {header.isPlaceholder
                                        ? null
                                        : flexRender(
                                              header.column.columnDef.header,
                                              header.getContext(),
                                          )}
                                </TableHead>
                            ))}
                        </TableRow>
                    ))}
                </TableHeader>
                <TableBody>
                    {table.getRowModel().rows.length ? (
                        table.getRowModel().rows.map((row) => (
                            <TableRow
                                key={row.id}
                                onClick={row.getToggleSelectedHandler()}
                            >
                                {row.getVisibleCells().map((cell) => (
                                    <TableCell key={cell.id} className="p-2">
                                        {flexRender(
                                            cell.column.columnDef.cell,
                                            cell.getContext(),
                                        )}
                                    </TableCell>
                                ))}
                            </TableRow>
                        ))
                    ) : (
                        <TableRow>
                            <TableCell
                                colSpan={table.getAllColumns().length}
                                className="h-24 text-center"
                            >
                                {intl.formatMessage({
                                    id: "logsheets.table.no-logsheets",
                                    defaultMessage: "No logsheets found.",
                                })}
                            </TableCell>
                        </TableRow>
                    )}
                </TableBody>
            </Table>
            <div className="flex flex-row justify-between">
                <p className="text-sm text-muted-foreground w-full">
                    {intl.formatMessage(
                        {
                            id: "logsheets.table.pagination.page-info",
                            defaultMessage:
                                "Page {currentPage} of {totalPages}",
                        },
                        {
                            currentPage:
                                table.getState().pagination.pageIndex + 1,
                            totalPages: table.getPageCount(),
                        },
                    )}
                </p>
                <Pagination className="justify-end">
                    <PaginationContent>
                        <PaginationItem>
                            <Button
                                variant="outline"
                                className="select-none cursor-pointer"
                                onClick={table.previousPage}
                                disabled={!table.getCanPreviousPage()}
                            >
                                <ChevronLeftIcon className="h-4 w-4" />
                                {intl.formatMessage({
                                    id: "logsheets.table.pagination.previous",
                                    defaultMessage: "Previous",
                                })}
                            </Button>
                        </PaginationItem>
                        <PaginationItem>
                            <Button
                                variant="outline"
                                className="select-none cursor-pointer"
                                onClick={table.nextPage}
                                disabled={!table.getCanNextPage()}
                            >
                                {intl.formatMessage({
                                    id: "logsheets.table.pagination.next",
                                    defaultMessage: "Next",
                                })}
                                <ChevronRightIcon className="h-4 w-4" />
                            </Button>
                        </PaginationItem>
                    </PaginationContent>
                </Pagination>
                <PreviewModal
                    isOpen={!!previewLogsheetId}
                    onClose={() => setPreviewLogsheetId(null)}
                    logsheetId={previewLogsheetId}
                    templateId={templateId}
                />
            </div>
        </div>
    );
};
