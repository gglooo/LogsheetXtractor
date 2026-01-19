import { Screen } from "@/components/screen";
import { Button } from "@/components/ui/button";
import { LogsheetsNavbar } from "@/modules/logsheets/components/navbar";
import { LogsheetTable } from "@/modules/logsheets/table/logsheet-table";
import { baseTemplateEditorPath } from "@/modules/template-editor/routes";
import { FileCog, UploadIcon } from "lucide-react";
import { useIntl } from "react-intl";
import { useNavigate, useParams } from "react-router-dom";

export const LogsheetsPage = () => {
    const { templateId } = useParams<{ templateId: string }>();

    const navigate = useNavigate();
    const intl = useIntl();

    if (!templateId) {
        return <div className="p-4">No template ID provided.</div>;
    }

    const handleEditTemplate = () => {
        navigate(`${baseTemplateEditorPath}/${templateId}`);
    };

    return (
        <>
            <LogsheetsNavbar />
            <Screen className="bg-background">
                <div className="flex flex-col flex-1 px-4">
                    <div className="flex flex-row gap-2 items-center">
                        <Button
                            variant="outline"
                            className="mb-4 self-start"
                            onClick={() => navigate("upload")}
                        >
                            <UploadIcon className="mr-2 h-4 w-4" />
                            {intl.formatMessage({
                                id: "logsheets.add.button",
                                defaultMessage: "Add logsheets",
                            })}
                        </Button>
                        <Button
                            variant="outline"
                            className="mb-4 self-start"
                            onClick={handleEditTemplate}
                        >
                            <FileCog className="mr-2 h-4 w-4" />
                            {intl.formatMessage({
                                id: "logsheets.add.button",
                                defaultMessage: "Edit template",
                            })}
                        </Button>
                    </div>
                    <LogsheetTable templateId={templateId} className="border" />
                </div>
            </Screen>
        </>
    );
};
