import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogTitle } from "@/components/ui/dialog";
import { Spinner } from "@/components/ui/spinner";
import { useFileDownloadMutation } from "@/modules/files/api";
import {
    useDeleteLogsheetMutation,
    useProcessLogsheetMutation,
} from "@/modules/logsheets/api";
import type { LogsheetListType } from "@/modules/logsheets/schema";
import {
    DownloadIcon,
    EyeIcon,
    FileCog,
    FileSignature,
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

export const LogsheetTableActions = ({
    logsheet,
    onPreview,
}: LogsheetTableActionsProps) => {
    const intl = useIntl();
    const navigate = useNavigate();
    const { templateId } = useParams<{ templateId: string }>();

    const fileDownloadMutation = useFileDownloadMutation();
    const deleteLogsheetMutation = useDeleteLogsheetMutation();
    const processLogsheetMutation = useProcessLogsheetMutation();

    const handleDelete = async () => {
        try {
            await deleteLogsheetMutation.mutateAsync(logsheet.id);
            toast.success("Logsheet was deleted.");
        } catch (error) {
            console.log("Error deleting logsheet:", error);
            toast.error("Failed to delete logsheet.");
        }
    };

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
                    onClick={() => onPreview(logsheet.file.id)}
                >
                    <EyeIcon className="h-4 w-4" />
                </Button>
                <Button
                    variant="ghost"
                    title="Proofread"
                    disabled={logsheet.status !== "NeedsReview"}
                    onClick={() =>
                        navigate(
                            `/templates/${templateId}/logsheets/${logsheet.id}/proofread`
                        )
                    }
                >
                    <FileSignature className="h-4 w-4" />
                </Button>
                <Button
                    variant="ghost"
                    title="Align"
                    onClick={() =>
                        navigate(
                            `/templates/${templateId}/logsheets/${logsheet.id}/align`
                        )
                    }
                >
                    <ScanLine className="h-4 w-4" />
                </Button>
                <Button
                    variant="ghost"
                    title="Download"
                    disabled={fileDownloadMutation.isPending}
                    onClick={async () =>
                        fileDownloadMutation.mutateAsync({
                            fileId: logsheet.file.id,
                        })
                    }
                >
                    <DownloadIcon className="h-4 w-4" />
                </Button>
                <Button
                    variant="ghost"
                    title="Delete"
                    disabled={deleteLogsheetMutation.isPending}
                    onClick={handleDelete}
                >
                    {deleteLogsheetMutation.isPending ? (
                        <Spinner />
                    ) : (
                        <TrashIcon className="h-4 w-4" />
                    )}
                </Button>
                {!logsheet.processedAt ? (
                    <Button
                        variant="outline"
                        disabled={processLogsheetMutation.isPending}
                        onClick={handleProcess}
                    >
                        {processLogsheetMutation.isPending ? (
                            <Spinner />
                        ) : (
                            <FileCog className="h-4 w-4" />
                        )}
                    </Button>
                ) : null}
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
