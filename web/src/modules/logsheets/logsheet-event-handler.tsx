import { useSignalR } from "@/modules/signalr/signalr-context";
import { useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";
import { useIntl } from "react-intl";
import { toast } from "sonner";

type LogsheetEvent = {
    logsheetId: string;
    isSuccess: boolean;
    errorMessage?: string;
};

export type LogsheetProcessingFinishedEvent = LogsheetEvent;

export type LogsheetAutomaticAlignmentFinishedEvent = LogsheetEvent;

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
                    {
                        id: "logsheet-processing-success",
                    },
                );
            } else {
                toast.error(
                    intl.formatMessage({
                        id: "logsheets.processing.error",
                        defaultMessage: "Logsheet processing failed",
                    }),
                    {
                        id: `logsheet-processing-error-${event.logsheetId}`,
                        description: event.errorMessage,
                    },
                );
            }
            await queryClient.invalidateQueries({ queryKey: ["logsheets"] });
        };

        const onLogsheetAutomaticAlignmentFinished = async (
            event: LogsheetAutomaticAlignmentFinishedEvent,
        ) => {
            if (!event.isSuccess) {
                toast.error(
                    intl.formatMessage({
                        id: "logsheets.automaticAlignment.error",
                        defaultMessage: "Logsheet automatic alignment failed",
                    }),
                    {
                        id: `logsheet-automatic-alignment-error-${event.logsheetId}`,
                        description: event.errorMessage,
                    },
                );
            }

            await queryClient.invalidateQueries({ queryKey: ["logsheets"] });
        };

        connection.on(
            "LogsheetProcessingFinished",
            onLogsheetProcessingFinished,
        );
        connection.on(
            "LogsheetAutomaticAlignmentFinished",
            onLogsheetAutomaticAlignmentFinished,
        );

        return () => {
            connection.off(
                "LogsheetProcessingFinished",
                onLogsheetProcessingFinished,
            );
            connection.off(
                "LogsheetAutomaticAlignmentFinished",
                onLogsheetAutomaticAlignmentFinished,
            );
        };
    }, [connection, queryClient, intl]);

    return null;
};
