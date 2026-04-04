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
import { useCredentialsStatus } from "@/modules/settings/api";
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
import { LogsheetStateMachine } from "../state-machine";

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
            toast.success(
                intl.formatMessage({
                    id: "logsheets.actions.delete.success",
                    defaultMessage: "Logsheet deleted successfully!",
                }),
            );
        } catch (error) {
            console.log("Error deleting logsheet:", error);
            toast.error(
                intl.formatMessage({
                    id: "logsheets.actions.delete.error",
                    defaultMessage: "Failed to delete logsheet.",
                }),
            );
        }
    };

    const isAnyActionPending =
        fileDownloadMutation.isPending ||
        deleteLogsheetMutation.isPending ||
        exportLogsheetMutation.isPending;

    const stateMachine = LogsheetStateMachine.fromStatus(logsheet.status);

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
                    onClick={async () => {
                        await fileDownloadMutation.mutateAsync({
                            fileId: logsheet.file.id,
                        });
                    }}
                    disabled={fileDownloadMutation.isPending}
                >
                    <DownloadIcon className="mr-2 h-4 w-4" />
                    {intl.formatMessage({
                        id: "logsheets.actions.download",
                        defaultMessage: "Download",
                    })}
                </DropdownMenuItem>
                <DropdownMenuItem
                    onClick={async () => {
                        await exportLogsheetMutation.mutateAsync({
                            logsheetId: logsheet.id,
                        });
                    }}
                    disabled={
                        exportLogsheetMutation.isPending ||
                        !stateMachine.canExport()
                    }
                >
                    <ArrowRightFromLineIcon className="mr-2 h-4 w-4" />
                    {intl.formatMessage({
                        id: "logsheets.actions.export",
                        defaultMessage: "Export proofreading data",
                    })}
                </DropdownMenuItem>
                <DropdownMenuItem
                    onClick={async () => {
                        await handleDelete();
                    }}
                    className="text-red-600 focus:text-red-600"
                    disabled={
                        deleteLogsheetMutation.isPending ||
                        !stateMachine.canDelete()
                    }
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
    const { data: credentialsStatus } = useCredentialsStatus();
    const isCredentialsMissing = credentialsStatus?.available === false;
    const stateMachine = LogsheetStateMachine.fromStatus(logsheet.status);

    const noCredentialsTooltip = intl.formatMessage({
        id: "settings.credentials.missing.tooltip",
        defaultMessage:
            "OCR Credentials required. Please configure them in Settings.",
    });

    const handleProcess = async () => {
        try {
            await processLogsheetMutation.mutateAsync(logsheet.id);
            toast.success(
                intl.formatMessage({
                    id: "logsheets.actions.process.success",
                    defaultMessage: "Logsheet was queued for processing.",
                }),
            );
        } catch (error) {
            console.log("Error processing logsheet:", error);
            toast.error(
                intl.formatMessage({
                    id: "logsheets.actions.process.error",
                    defaultMessage: "Failed to queue logsheet for processing.",
                }),
            );
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
                    disabled={
                        !stateMachine.canProofread() || isCredentialsMissing
                    }
                    onClick={() => {
                        navigate(
                            `/templates/${templateId}/logsheets/${logsheet.id}/proofread`,
                        );
                    }}
                    tooltip={
                        isCredentialsMissing
                            ? noCredentialsTooltip
                            : intl.formatMessage({
                                  id: "logsheets.actions.proofread",
                                  defaultMessage: "Proofread",
                              })
                    }
                >
                    <FileSignature className="h-4 w-4" />
                </Button>
                <Button
                    variant="ghost"
                    title={intl.formatMessage({
                        id: "logsheets.actions.align",
                        defaultMessage: "Align",
                    })}
                    disabled={!stateMachine.canAlign()}
                    onClick={() => {
                        navigate(
                            `/templates/${templateId}/logsheets/${logsheet.id}/align`,
                        );
                    }}
                    tooltip={intl.formatMessage({
                        id: "logsheets.actions.align",
                        defaultMessage: "Align",
                    })}
                >
                    <ScanLine className="h-4 w-4" />
                </Button>

                <ActionsInDialog logsheet={logsheet} />

                {stateMachine.canProcess() && (
                    <Button
                        variant="outline"
                        disabled={
                            processLogsheetMutation.isPending ||
                            isCredentialsMissing
                        }
                        onClick={handleProcess}
                        title={intl.formatMessage({
                            id: "logsheets.actions.process",
                            defaultMessage: "Process",
                        })}
                        tooltip={
                            isCredentialsMissing
                                ? noCredentialsTooltip
                                : intl.formatMessage({
                                      id: "logsheets.actions.process",
                                      defaultMessage: "Process",
                                  })
                        }
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
