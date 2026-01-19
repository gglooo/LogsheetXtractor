import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogTitle } from "@/components/ui/dialog";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Spinner } from "@/components/ui/spinner";
import { useFileDownloadMutation } from "@/modules/files/api";
import {
    useDeleteLogsheetMutation,
    useExportLogsheetMutation,
    useProcessLogsheetMutation,
} from "@/modules/logsheets/api";
import type { LogsheetListType } from "@/modules/logsheets/schema";
import {
    ArrowRightFromLineIcon,
    DownloadIcon,
    EyeIcon,
    FileCog,
    FileSignature,
    MoreHorizontal,
    ScanLine,
    TrashIcon,
} from "lucide-react";
import { useIntl } from "react-intl";
import { useNavigate, useParams } from "react-router-dom";
import { toast } from "sonner";

export type LogsheetTableActionsProps = {
    logsheet: LogsheetListType;
    onPreview: (id: string) => void;
};

const ActionsInDialog = ({ logsheet }: { logsheet: LogsheetListType }) => {
    const intl = useIntl();
    const fileDownloadMutation = useFileDownloadMutation();
    const deleteLogsheetMutation = useDeleteLogsheetMutation();
    const exportLogsheetMutation = useExportLogsheetMutation();

    const handleDelete = async () => {
        try {
            await deleteLogsheetMutation.mutateAsync(logsheet.id);
            toast.success("Logsheet was deleted.");
        } catch (error) {
            console.log("Error deleting logsheet:", error);
            toast.error("Failed to delete logsheet.");
        }
    };

    const isAnyActionPending =
        fileDownloadMutation.isPending ||
        deleteLogsheetMutation.isPending ||
        exportLogsheetMutation.isPending;

    return (
        <DropdownMenu>
            <DropdownMenuTrigger asChild>
                <Button
                    variant="ghost"
                    className="h-8 w-8 p-0"
                    title={intl.formatMessage({
                        id: "common.actions.more",
                        defaultMessage: "More actions",
                    })}
                >
                    {isAnyActionPending ? (
                        <Spinner />
                    ) : (
                        <MoreHorizontal className="h-4 w-4" />
                    )}
                </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
                <DropdownMenuItem
                    onClick={async () =>
                        fileDownloadMutation.mutateAsync({
                            fileId: logsheet.file.id,
                        })
                    }
                    disabled={fileDownloadMutation.isPending}
                >
                    <DownloadIcon className="mr-2 h-4 w-4" />
                    {intl.formatMessage({
                        id: "logsheets.actions.download",
                        defaultMessage: "Download",
                    })}
                </DropdownMenuItem>
                <DropdownMenuItem
                    onClick={async () =>
                        exportLogsheetMutation.mutateAsync({
                            logsheetId: logsheet.id,
                        })
                    }
                    disabled={exportLogsheetMutation.isPending}
                >
                    <ArrowRightFromLineIcon className="mr-2 h-4 w-4" />
                    {intl.formatMessage({
                        id: "logsheets.actions.export",
                        defaultMessage: "Export",
                    })}
                </DropdownMenuItem>
                <DropdownMenuItem
                    onClick={handleDelete}
                    className="text-red-600 focus:text-red-600"
                    disabled={deleteLogsheetMutation.isPending}
                >
                    <TrashIcon className="mr-2 h-4 w-4" />
                    {intl.formatMessage({
                        id: "logsheets.actions.delete",
                        defaultMessage: "Delete",
                    })}
                </DropdownMenuItem>
            </DropdownMenuContent>
        </DropdownMenu>
    );
};

export const LogsheetTableActions = ({
    logsheet,
    onPreview,
}: LogsheetTableActionsProps) => {
    const intl = useIntl();
    const navigate = useNavigate();
    const { templateId } = useParams<{ templateId: string }>();

    const processLogsheetMutation = useProcessLogsheetMutation();

    const handleProcess = async () => {
        try {
            await processLogsheetMutation.mutateAsync(logsheet.id);
            toast.success("Logsheet was processed.");
        } catch (error) {
            console.log("Error processing logsheet:", error);
            toast.error("Failed to process logsheet.");
        }
    };

    return (
        <>
            <div className="flex gap-2">
                <Button
                    variant="ghost"
                    title={intl.formatMessage({
                        id: "logsheets.actions.preview",
                        defaultMessage: "Preview",
                    })}
                    onClick={() => onPreview(logsheet.id)}
                    tooltip={intl.formatMessage({
                        id: "logsheets.actions.preview",
                        defaultMessage: "Preview",
                    })}
                >
                    <EyeIcon className="h-4 w-4" />
                </Button>
                <Button
                    variant="ghost"
                    title={intl.formatMessage({
                        id: "logsheets.actions.proofread",
                        defaultMessage: "Proofread",
                    })}
                    disabled={logsheet.status !== "NeedsReview"}
                    onClick={() =>
                        navigate(
                            `/templates/${templateId}/logsheets/${logsheet.id}/proofread`,
                        )
                    }
                    tooltip={intl.formatMessage({
                        id: "logsheets.actions.proofread",
                        defaultMessage: "Proofread",
                    })}
                >
                    <FileSignature className="h-4 w-4" />
                </Button>
                <Button
                    variant="ghost"
                    title={intl.formatMessage({
                        id: "logsheets.actions.align",
                        defaultMessage: "Align",
                    })}
                    onClick={() =>
                        navigate(
                            `/templates/${templateId}/logsheets/${logsheet.id}/align`,
                        )
                    }
                    tooltip={intl.formatMessage({
                        id: "logsheets.actions.align",
                        defaultMessage: "Align",
                    })}
                >
                    <ScanLine className="h-4 w-4" />
                </Button>

                <ActionsInDialog logsheet={logsheet} />

                {!logsheet.processedAt && (
                    <Button
                        variant="outline"
                        disabled={processLogsheetMutation.isPending}
                        onClick={handleProcess}
                        title={intl.formatMessage({
                            id: "logsheets.actions.process",
                            defaultMessage: "Process",
                        })}
                        tooltip={intl.formatMessage({
                            id: "logsheets.actions.process",
                            defaultMessage: "Process",
                        })}
                    >
                        {processLogsheetMutation.isPending ? (
                            <Spinner />
                        ) : (
                            <FileCog className="h-4 w-4" />
                        )}
                    </Button>
                )}
            </div>
            <Dialog open={processLogsheetMutation.isPending}>
                <DialogContent>
                    <DialogTitle>
                        {intl.formatMessage({
                            id: "logsheets.actions.processing",
                            defaultMessage: "Processing logsheet",
                        })}
                    </DialogTitle>
                    <Spinner className="mt-4" />
                </DialogContent>
            </Dialog>
        </>
    );
};
