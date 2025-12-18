import { baseDashboardPath, DashboardRoutes } from "@/modules/dashboard/routes";
import {
    baseTemplateEditorPath,
    TemplateEditorRoutes,
} from "@/modules/template-editor/routes";
import { useIntl } from "react-intl";
import { Navigate, Route, Routes } from "react-router-dom";
import { Toaster } from "sonner";

function App() {
    const intl = useIntl();

    return (
        <div className="min-h-screen bg-background text-foreground font-mono">
            <Routes>
                <Route
                    path="/"
                    element={<Navigate to={`${baseDashboardPath}`} replace />}
                />

                <Route
                    path={`${baseDashboardPath}/*`}
                    element={<DashboardRoutes />}
                />
                <Route
                    path={`${baseTemplateEditorPath}/*`}
                    element={<TemplateEditorRoutes />}
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
        </div>
    );
}

export default App;
