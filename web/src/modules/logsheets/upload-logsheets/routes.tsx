import { LogsheetsUploadNavbar } from "@/modules/logsheets/upload-logsheets/components/navbar";
import { UploadLogsheetsProvider } from "@/modules/logsheets/upload-logsheets/context/upload-logsheets-context";
import { UploadLogsheetPage } from "@/modules/logsheets/upload-logsheets/page";
import { Route, Routes, useParams } from "react-router-dom";

export const baseUploadPath = "/upload";

export const UploadLogsheetRoutes = () => {
    const { templateId } = useParams<{ templateId: string }>();

    if (!templateId) return null;

    return (
        <UploadLogsheetsProvider templateId={templateId}>
            <LogsheetsUploadNavbar />
            <Routes>
                <Route index element={<UploadLogsheetPage />} />
            </Routes>
        </UploadLogsheetsProvider>
    );
};
