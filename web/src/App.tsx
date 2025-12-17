import { Navbar } from "@/components/navbar";
import { DashboardRoutes } from "@/modules/dashboard/routes";
import { useIntl } from "react-intl";
import { Navigate, Route, Routes } from "react-router-dom";
import { Toaster } from "sonner";

function App() {
    const intl = useIntl();

    return (
        <div className="min-h-screen bg-background text-foreground font-mono">
            <Navbar />
            <Routes>
                <Route
                    path="/"
                    element={<Navigate to="/dashboard" replace />}
                />

                <Route path="/dashboard/*" element={<DashboardRoutes />} />

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
