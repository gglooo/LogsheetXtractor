import { Checkbox } from "@/components/ui/checkbox";
import { LogsheetTableActions } from "@/modules/logsheets/actions/table-actions";
import { ProcessedBadge } from "@/modules/logsheets/components/processed-badge";
import { LogsheetStatusBadge } from "@/modules/logsheets/components/status-badge";
import type { LogsheetListType } from "@/modules/logsheets/schema";
import { type RowData, createColumnHelper } from "@tanstack/react-table";
import { FileText } from "lucide-react";
import { useIntl } from "react-intl";

const columnHelper = createColumnHelper<LogsheetListType>();

export type LogsheetTableMeta = {
    onPreview: (id: string) => void;
};

declare module "@tanstack/react-table" {
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    interface TableMeta<TData extends RowData> {
        onPreview: (id: string) => void;
    }
}

export const useLogsheetsColumns = () => {
    const intl = useIntl();

    return [
        columnHelper.display({
            id: "select",
            header: ({ table }) => (
                <Checkbox
                    checked={table.getIsAllPageRowsSelected()}
                    onClick={table.getToggleAllRowsSelectedHandler()}
                >
                    {intl.formatMessage({
                        id: "logsheets.table.columns.select",
                        defaultMessage: "Select all",
                    })}
                </Checkbox>
            ),
            cell: ({ row }) => (
                <Checkbox
                    onClick={row.getToggleSelectedHandler()}
                    checked={row.getIsSelected()}
                />
            ),
        }),
        columnHelper.accessor("file.fileName", {
            header: intl.formatMessage({
                id: "logsheets.table.columns.fileName",
                defaultMessage: "File Name",
            }),
            cell: (info) => (
                <div className="flex items-center">
                    <div className="inline mr-2 bg-muted p-2 rounded-md">
                        <FileText className="inline w-4 h-4" />
                    </div>
                    <p className="font-semibold">{info.getValue()}</p>
                </div>
            ),
        }),
        columnHelper.accessor("status", {
            header: intl.formatMessage({
                id: "logsheets.table.columns.status",
                defaultMessage: "Status",
            }),
            cell: (info) => (
                <LogsheetStatusBadge
                    status={info.getValue()}
                    errorMessage={info.row.original.errorMessage}
                />
            ),
        }),
        columnHelper.accessor("processedAt", {
            header: intl.formatMessage({
                id: "logsheets.table.columns.processed",
                defaultMessage: "Processed",
            }),
            cell: (info) => <ProcessedBadge processedAt={info.getValue()} />,
        }),
        columnHelper.accessor("isFrontAligned", {
            id: "alignment",
            header: intl.formatMessage({
                id: "logsheets.table.columns.alignmentStatus",
                defaultMessage: "Aligned",
            }),
            cell: (info) => {
                const isFront = info.row.original.isFrontAligned;
                const isBack = info.row.original.isBackAligned;

                if (isFront && isBack) {
                    return intl.formatMessage({
                        id: "logsheets.table.columns.alignmentStatus.both",
                        defaultMessage: "Both sides",
                    });
                }
                if (isFront) {
                    return intl.formatMessage({
                        id: "logsheets.table.columns.alignmentStatus.front",
                        defaultMessage: "Front",
                    });
                }
                if (isBack) {
                    return intl.formatMessage({
                        id: "logsheets.table.columns.alignmentStatus.back",
                        defaultMessage: "Back",
                    });
                }
                return intl.formatMessage({
                    id: "logsheets.table.columns.alignmentStatus.no",
                    defaultMessage: "No",
                });
            },
        }),
        columnHelper.display({
            id: "actions",
            header: intl.formatMessage({
                id: "logsheets.table.columns.actions",
                defaultMessage: "Actions",
            }),
            cell: ({ row, table }) => (
                <LogsheetTableActions
                    logsheet={row.original}
                    onPreview={table.options.meta?.onPreview ?? (() => {})}
                />
            ),
        }),
    ];
};
