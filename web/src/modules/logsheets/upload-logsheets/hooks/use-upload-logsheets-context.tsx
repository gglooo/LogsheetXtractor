import { createContext, useContext } from "react";

export type UploadLogsheetsContextType = {
    files: File[];
    setFiles: (files: File[]) => void;
    handleUpload: () => Promise<void>;
    isUploading: boolean;
};

export const UploadLogsheetsContext = createContext<
    UploadLogsheetsContextType | undefined
>(undefined);

export const useUploadLogsheetsContext = () => {
    const context = useContext(UploadLogsheetsContext);
    if (!context) {
        throw new Error(
            "useUploadLogsheetsContext must be used within an UploadLogsheetsProvider"
        );
    }
    return context;
};
