import { Toaster } from "@/components/ui/sonner";
import { baseDashboardPath, DashboardRoutes } from "@/modules/dashboard/routes";
import { LogsheetEventHandler } from "@/modules/logsheets/logsheet-event-handler";
import { GamifiedProofreadingPage } from "@/modules/logsheets/proofreading/gamified-proofreading-page";
import { baseLogsheetsPath, LogsheetsRoutes } from "@/modules/logsheets/routes";
import { SettingsPage } from "@/modules/settings/page";
import { SignalRProvider } from "@/modules/signalr/signalr-provider";
import {
    baseTemplateEditorPath,
    TemplateEditorRoutes,
} from "@/modules/template-editor/routes";
import { useIntl } from "react-intl";
import { Navigate, Route, Routes } from "react-router-dom";

function App() {
    const intl = useIntl();

    return (
        <SignalRProvider>
            <div className="min-h-screen bg-background text-foreground">
                <Routes>
                    <Route
                        path="/"
                        element={
                            <Navigate to={`${baseDashboardPath}`} replace />
                        }
                    />

                    <Route
                        path={`${baseDashboardPath}/*`}
                        element={<DashboardRoutes />}
                    />
                    <Route path="/settings" element={<SettingsPage />} />
                    <Route
                        path={`${baseTemplateEditorPath}/*`}
                        element={<TemplateEditorRoutes />}
                    />
                    <Route
                        path={`${baseLogsheetsPath}/gamified-proofread`}
                        element={<GamifiedProofreadingPage />}
                    />
                    <Route
                        path={`/templates/:templateId${baseLogsheetsPath}/*`}
                        element={<LogsheetsRoutes />}
                    />
                    <Route
                        path="*"
                        element={
                            <div className="p-10">
                                {intl.formatMessage({
                                    id: "app.notFound",
                                    defaultMessage: "404 - Not Found",
                                })}
                            </div>
                        }
                    />
                </Routes>
                <Toaster />
                <LogsheetEventHandler />
            </div>
        </SignalRProvider>
    );
}

export default App;
