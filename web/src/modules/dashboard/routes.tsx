import { DashboardPage } from "@/modules/dashboard/page";
import { Navigate, Route, Routes } from "react-router-dom";

export const DashboardRoutes = () => {
    return (
        <Routes>
            <Route path="/" element={<DashboardPage />} />
            <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
    );
};
