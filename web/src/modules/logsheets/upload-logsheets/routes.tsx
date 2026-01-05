import { AlignLogsheetsPage } from "@/modules/logsheets/upload-logsheets/align-logsheets/page";
import { LogsheetsUploadNavbar } from "@/modules/logsheets/upload-logsheets/components/navbar";
import { UploadLogsheetsProvider } from "@/modules/logsheets/upload-logsheets/context/upload-logsheets-context";
import { UploadLogsheetPage } from "@/modules/logsheets/upload-logsheets/page";
import { Route, Routes } from "react-router-dom";

export const baseUploadPath = "/upload";

export const UploadLogsheetRoutes = () => {
    return (
        <UploadLogsheetsProvider>
            <LogsheetsUploadNavbar />
            <Routes>
                <Route index element={<UploadLogsheetPage />} />
                <Route path="align" element={<AlignLogsheetsPage />} />
            </Routes>
        </UploadLogsheetsProvider>
    );
};
