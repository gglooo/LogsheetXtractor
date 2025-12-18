import { Navbar } from "@/components/navbar";
import { DashboardPage } from "@/modules/dashboard/page";
import { Navigate, Route, Routes } from "react-router-dom";

export const baseDashboardPath = "/dashboard";

export const DashboardRoutes = () => {
    return (
        <>
            <Navbar />
            <Routes>
                <Route path="/" element={<DashboardPage />} />
                <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
        </>
    );
};
