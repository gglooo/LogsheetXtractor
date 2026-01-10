import { useUploadFileMutation } from "@/modules/files/api";
import { useUploadLogsheetsMutation } from "@/modules/logsheets/api";
import { UploadLogsheetsContext } from "@/modules/logsheets/upload-logsheets/hooks/use-upload-logsheets-context";
import { type ReactNode, useState } from "react";
import { useIntl } from "react-intl";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";

export const UploadLogsheetsProvider = ({
    children,
    templateId,
}: {
    children: ReactNode;
    templateId: string;
}) => {
    const intl = useIntl();
    const navigate = useNavigate();
    const [files, setFiles] = useState<File[]>([]);

    const uploadFileMutation = useUploadFileMutation();
    const uploadLogsheetsMutation = useUploadLogsheetsMutation();

    const isUploading =
        uploadFileMutation.isPending || uploadLogsheetsMutation.isPending;

    const handleUpload = async () => {
        if (files.length === 0) return;

        try {
            const fileIds: string[] = [];

            for (const file of files) {
                const uploadedFile = await uploadFileMutation.mutateAsync(file);
                fileIds.push(uploadedFile.id);
            }

            await uploadLogsheetsMutation.mutateAsync({
                templateId,
                fileIds,
            });

            toast.success(
                intl.formatMessage({
                    id: "logsheets.upload.success",
                    defaultMessage: "Logsheets uploaded successfully",
                })
            );
            navigate(`/templates/${templateId}/logsheets`);
        } catch (error) {
            console.error("Upload failed", error);
            toast.error(
                intl.formatMessage({
                    id: "logsheets.upload.error",
                    defaultMessage: "Failed to upload logsheets",
                })
            );
        }
    };

    return (
        <UploadLogsheetsContext.Provider
            value={{
                files,
                setFiles,
                handleUpload,
                isUploading,
            }}
        >
            {children}
        </UploadLogsheetsContext.Provider>
    );
};
