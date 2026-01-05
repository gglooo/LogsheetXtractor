import { useUploadFileMutation } from "@/modules/files/api";
import { useUploadLogsheetsMutation } from "@/modules/logsheets/api";
import { useUploadLogsheetsContext } from "@/modules/logsheets/upload-logsheets/hooks/use-upload-logsheets-context";
import { useCallback, useEffect } from "react";
import { useIntl } from "react-intl";
import { toast } from "sonner";

export const useLogsheetUploadNextHandler = (templateId: string) => {
    const { logsheets, setLogsheets, registerNextHandler } =
        useUploadLogsheetsContext();
    const uploadLogsheetMutation = useUploadLogsheetsMutation();
    const uploadFileMutation = useUploadFileMutation();

    const intl = useIntl();

    const nextHandler = useCallback(async () => {
        if (logsheets.length === 0) {
            return false;
        }

        try {
            const uploadedFiles = await Promise.all(
                logsheets.map((ls) =>
                    uploadFileMutation.mutateAsync(ls.rawFile)
                )
            );

            const uploadResults = await uploadLogsheetMutation.mutateAsync({
                templateId: templateId!,
                fileIds: uploadedFiles.map((f) => f.id),
            });

            setLogsheets(
                logsheets.map((ls, index) => ({
                    ...ls,
                    file: uploadedFiles[index],
                    logsheet: uploadResults[index],
                }))
            );

            toast.success(
                intl.formatMessage({
                    id: "logsheets.upload.success",
                    defaultMessage: "Logsheets uploaded.",
                })
            );

            return true;
        } catch (error) {
            toast.error(
                intl.formatMessage({
                    id: "logsheets.upload.errors.upload-failed",
                    defaultMessage:
                        "Failed to upload logsheets. Please try again.",
                })
            );
            console.log("Upload logsheets error:", error);
            return false;
        }
    }, [
        intl,
        logsheets,
        setLogsheets,
        templateId,
        uploadFileMutation,
        uploadLogsheetMutation,
    ]);

    useEffect(() => {
        const unregister = registerNextHandler(nextHandler);
        return () => {
            unregister();
        };
    }, [registerNextHandler, nextHandler]);

    return {
        files: logsheets.map((ls) => ls.rawFile),
        setFiles: (files: File[]) =>
            setLogsheets(
                files.map((f) => ({
                    rawFile: f,
                }))
            ),
    };
};
