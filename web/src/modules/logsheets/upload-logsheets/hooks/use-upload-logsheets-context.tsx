import type { FileType } from "@/modules/files/schema";
import type { LogsheetAlignmentData } from "@/modules/logsheets/schema";
import { createContext, useContext } from "react";

export type ContextLogsheet = {
    file?: FileType;
    rawFile: File;
    alignmentData?: LogsheetAlignmentData;
};

export type UploadLogsheetsContextType = {
    logsheets: ContextLogsheet[];
    addLogsheets: (files: File[]) => void;
    setLogsheets: (logsheets: ContextLogsheet[]) => void;
    removeLogsheet: (index: number) => void;
    setAlignment: (id: string, alignment: LogsheetAlignmentData) => void;
    clearLogsheets: () => void;
    handleContinue: () => Promise<void>;
    canContinue: boolean;
    registerNextHandler: (handler: () => Promise<boolean | void>) => () => void;
    submitLogsheets: () => Promise<void>;
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
