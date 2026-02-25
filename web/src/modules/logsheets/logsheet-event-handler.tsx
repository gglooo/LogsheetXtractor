import { useSignalR } from "@/modules/signalr/signalr-context";
import { useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";
import { useIntl } from "react-intl";
import { toast } from "sonner";

export type LogsheetProcessingFinishedEvent = {
    logsheetId: string;
    isSuccess: boolean;
    errorMessage?: string;
};

export type BatchProcessingFinishedEvent = {
    processedLogsheetIds: string[];
    failedLogsheetIds: string[];
    errorMessages: string[];
};

export const LogsheetEventHandler = () => {
    const { connection } = useSignalR();
    const queryClient = useQueryClient();
    const intl = useIntl();

    useEffect(() => {
        if (!connection) return;

        const onLogsheetProcessingFinished = async (
            event: LogsheetProcessingFinishedEvent,
        ) => {
            if (event.isSuccess) {
                toast.success(
                    intl.formatMessage({
                        id: "logsheets.processing.success",
                        defaultMessage:
                            "Logsheet processing completed successfully",
                    }),
                );
            } else {
                toast.error(
                    intl.formatMessage({
                        id: "logsheets.processing.error",
                        defaultMessage: "Logsheet processing failed",
                    }),
                    {
                        description: event.errorMessage,
                    },
                );
            }
            await queryClient.invalidateQueries({ queryKey: ["logsheets"] });
        };

        const onBatchProcessingFinished = async (
            event: BatchProcessingFinishedEvent,
        ) => {
            if (event.failedLogsheetIds.length === 0) {
                toast.success(
                    intl.formatMessage({
                        id: "logsheets.batchProcessing.success",
                        defaultMessage:
                            "Batch processing completed successfully",
                    }),
                );
            } else if (event.processedLogsheetIds.length > 0) {
                toast.warning(
                    intl.formatMessage(
                        {
                            id: "logsheets.batchProcessing.partialSuccess",
                            defaultMessage:
                                "Batch processing finished with {errors} errors",
                        },
                        { errors: event.failedLogsheetIds.length },
                    ),
                    {
                        description: event.errorMessages[0],
                    },
                );
            } else {
                toast.error(
                    intl.formatMessage({
                        id: "logsheets.batchProcessing.error",
                        defaultMessage: "Batch processing failed",
                    }),
                    {
                        description:
                            event.errorMessages.length > 0
                                ? event.errorMessages[0]
                                : undefined,
                    },
                );
            }
            await queryClient.invalidateQueries({ queryKey: ["logsheets"] });
        };

        connection.on(
            "LogsheetProcessingFinished",
            onLogsheetProcessingFinished,
        );
        connection.on("BatchProcessingFinished", onBatchProcessingFinished);

        return () => {
            connection.off(
                "LogsheetProcessingFinished",
                onLogsheetProcessingFinished,
            );
            connection.off(
                "BatchProcessingFinished",
                onBatchProcessingFinished,
            );
        };
    }, [connection, queryClient, intl]);

    return null;
};
