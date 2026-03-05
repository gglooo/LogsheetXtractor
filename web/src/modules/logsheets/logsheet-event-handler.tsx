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

        connection.on(
            "LogsheetProcessingFinished",
            onLogsheetProcessingFinished,
        );

        return () => {
            connection.off(
                "LogsheetProcessingFinished",
                onLogsheetProcessingFinished,
            );
        };
    }, [connection, queryClient, intl]);

    return null;
};
