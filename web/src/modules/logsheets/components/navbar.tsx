import { NavbarContainer } from "@/components/navbar-container";
import { baseDashboardPath } from "@/modules/dashboard/routes";
import { ArrowLeft } from "lucide-react";
import { useIntl } from "react-intl";
import { useNavigate } from "react-router-dom";

export const LogsheetsNavbar = () => {
    const navigate = useNavigate();

    const intl = useIntl();

    return (
        <NavbarContainer>
            <div className="flex flex-row items-center gap-4">
                <ArrowLeft
                    className="cursor-pointer"
                    onClick={() => navigate(baseDashboardPath)}
                />
                <div className="flex justify-between w-full">
                    <div className="p-4 text-lg font-bold">
                        {intl.formatMessage({
                            id: "logsheets.title",
                            defaultMessage: "Logsheets",
                        })}
                    </div>
                    <div className="flex items-center gap-2 p-4"></div>
                </div>
            </div>
        </NavbarContainer>
    );
};
