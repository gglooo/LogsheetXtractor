import { resolveParentPath } from "@/lib/navigation";
import { useLocation, useNavigate } from "react-router-dom";

export const useNavigateUp = () => {
    const navigate = useNavigate();
    const { pathname } = useLocation();

    return () => {
        const parent = resolveParentPath(pathname) ?? "/dashboard";
        navigate(parent);
    };
};
