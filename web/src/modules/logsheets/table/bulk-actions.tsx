import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogTitle } from "@/components/ui/dialog";
import { Spinner } from "@/components/ui/spinner";
import { cn } from "@/lib/utils";
import {
    useDeleteLogsheetsMutation,
    useProcessLogsheetsMutation,
} from "@/modules/logsheets/api";
import { FileCog, TrashIcon } from "lucide-react";
import { useIntl } from "react-intl";
import { toast } from "sonner";

export const LogsheetTableBulkActions = ({
    selectedLogsheetIds,
    onClearSelection,
    className,
}: {
    selectedLogsheetIds: string[];
    onClearSelection: () => void;
    className?: string;
}) => {
    const intl = useIntl();

    const deleteLogsheetsMutation = useDeleteLogsheetsMutation();
    const processLogsheetsMutation = useProcessLogsheetsMutation();

    if (selectedLogsheetIds.length === 0) {
        return null;
    }

    const handleDelete = async () => {
        try {
            await deleteLogsheetsMutation.mutateAsync(selectedLogsheetIds);
            toast.success(
                intl.formatMessage({
                    id: "logsheets.bulk.delete.success",
                    defaultMessage: "Logsheets deleted successfully",
                })
            );
            onClearSelection();
        } catch (error) {
            console.error("Error deleting logsheets:", error);
            toast.error(
                intl.formatMessage({
                    id: "logsheets.bulk.delete.error",
                    defaultMessage: "Failed to delete logsheets",
                })
            );
        }
    };

    const handleProcess = async () => {
        try {
            await processLogsheetsMutation.mutateAsync(selectedLogsheetIds);
            toast.success(
                intl.formatMessage({
                    id: "logsheets.bulk.process.success",
                    defaultMessage: "Logsheets were processed",
                })
            );
            onClearSelection();
        } catch (error) {
            console.error("Error processing logsheets:", error);
            toast.error(
                intl.formatMessage({
                    id: "logsheets.bulk.process.error",
                    defaultMessage: "Failed to process logsheets",
                })
            );
        }
    };

    return (
        <div
            className={cn(
                "flex items-center justify-between gap-4 p-4 bg-muted/50 border rounded-md",
                className
            )}
        >
            <p className="text-sm font-medium">
                {intl.formatMessage(
                    {
                        id: "logsheets.table.bulkActions.selectedCount",
                        defaultMessage: "{count} logsheet(s) selected",
                    },
                    { count: selectedLogsheetIds.length }
                )}
            </p>
            <div className="flex gap-2">
                <Button
                    variant="ghost"
                    size="sm"
                    onClick={handleDelete}
                    disabled={deleteLogsheetsMutation.isPending}
                >
                    {deleteLogsheetsMutation.isPending ? (
                        <Spinner className="mr-2 h-4 w-4" />
                    ) : (
                        <TrashIcon className="mr-2 h-4 w-4" />
                    )}
                    {intl.formatMessage({
                        id: "logsheets.bulk.delete",
                        defaultMessage: "Delete",
                    })}
                </Button>
                <Button
                    variant="default"
                    size="sm"
                    onClick={handleProcess}
                    disabled={processLogsheetsMutation.isPending}
                >
                    {processLogsheetsMutation.isPending ? (
                        <Spinner className="mr-2 h-4 w-4" />
                    ) : (
                        <FileCog className="mr-2 h-4 w-4" />
                    )}
                    {intl.formatMessage({
                        id: "logsheets.bulk.process",
                        defaultMessage: "Process",
                    })}
                </Button>
            </div>

            <Dialog open={processLogsheetsMutation.isPending}>
                <DialogContent>
                    <DialogTitle>
                        {intl.formatMessage({
                            id: "logsheets.actions.processing",
                            defaultMessage: "Processing logsheets",
                        })}
                    </DialogTitle>
                    <div className="flex justify-center p-4">
                        <Spinner />
                    </div>
                </DialogContent>
            </Dialog>
        </div>
    );
};
