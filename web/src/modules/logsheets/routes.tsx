import { LogsheetAlignmentPage } from "@/modules/logsheets/alignment/alignment-page";
import { LogsheetsPage } from "@/modules/logsheets/page";
import { ProofreadingPage } from "@/modules/logsheets/proofreading/proofreading-page";
import { UploadLogsheetRoutes } from "@/modules/logsheets/upload-logsheets/routes";
import { Route, Routes } from "react-router-dom";

export const baseLogsheetsPath = "/logsheets";

export const LogsheetsRoutes = () => {
    return (
        <div className="flex flex-col h-screen overflow-hidden bg-background">
            <Routes>
                <Route index element={<LogsheetsPage />} />
                <Route path="upload/*" element={<UploadLogsheetRoutes />} />
                <Route path=":id/align" element={<LogsheetAlignmentPage />} />
                <Route path=":id/proofread" element={<ProofreadingPage />} />
            </Routes>
        </div>
    );
};
