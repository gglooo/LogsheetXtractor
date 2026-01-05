import { NavbarContainer } from "@/components/navbar-container";
import { Button } from "@/components/ui/button";
import { useUploadLogsheetsContext } from "@/modules/logsheets/upload-logsheets/hooks/use-upload-logsheets-context";
import { ArrowLeft } from "lucide-react";
import { useIntl } from "react-intl";
import { useNavigate } from "react-router-dom";

export const LogsheetsUploadNavbar = () => {
    const navigate = useNavigate();

    const { canContinue, handleContinue } = useUploadLogsheetsContext();

    const intl = useIntl();

    return (
        <NavbarContainer>
            <div className="flex flex-row items-center gap-4">
                <ArrowLeft
                    className="cursor-pointer"
                    onClick={() => navigate(-1)}
                />
                <div className="flex w-full flex-col p-4">
                    <div className="text-lg font-bold">
                        {intl.formatMessage({
                            id: "logsheets.upload.title",
                            defaultMessage: "Upload logsheets",
                        })}
                    </div>
                </div>
            </div>
            <Button disabled={!canContinue} onClick={handleContinue}>
                {intl.formatMessage({
                    id: "logsheets.upload.actions.upload",
                    defaultMessage: "Continue",
                })}
            </Button>
        </NavbarContainer>
    );
};
