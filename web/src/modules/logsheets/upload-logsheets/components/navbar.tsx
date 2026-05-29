import { NavbarContainer } from "@/components/navbar-container";
import { Button } from "@/components/ui/button";
import { Spinner } from "@/components/ui/spinner";
import { useNavigateUp } from "@/hooks/use-navigate-up";
import { useUploadLogsheetsContext } from "@/modules/logsheets/upload-logsheets/hooks/use-upload-logsheets-context";
import { ArrowLeft } from "lucide-react";
import { useIntl } from "react-intl";

export const LogsheetsUploadNavbar = () => {
    const navigateUp = useNavigateUp();

    const { handleUpload, files, isUploading } = useUploadLogsheetsContext();

    const intl = useIntl();

    return (
        <NavbarContainer>
            <div className="flex flex-row items-center gap-4">
                <ArrowLeft
                    className="cursor-pointer"
                    onClick={() => navigateUp()}
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
            <Button disabled={files.length === 0} onClick={handleUpload}>
                {isUploading ? <Spinner /> : null}
                {intl.formatMessage({
                    id: "logsheets.upload.actions.upload",
                    defaultMessage: "Upload",
                })}
            </Button>
        </NavbarContainer>
    );
};
