import { Screen } from "@/components/screen";
import { Button } from "@/components/ui/button";
import { LogsheetsNavbar } from "@/modules/logsheets/components/navbar";
import { LogsheetTable } from "@/modules/logsheets/table/logsheet-table";
import { useIntl } from "react-intl";
import { useNavigate, useParams } from "react-router-dom";

export const LogsheetsPage = () => {
    const { templateId } = useParams<{ templateId: string }>();

    const navigate = useNavigate();
    const intl = useIntl();

    if (!templateId) {
        return <div className="p-4">No template ID provided.</div>;
    }

    return (
        <>
            <LogsheetsNavbar />
            <Screen className="bg-background">
                <div className="flex flex-col flex-1 px-4">
                    <Button
                        variant="outline"
                        className="mb-4 self-start"
                        onClick={() => navigate("upload")}
                    >
                        {intl.formatMessage({
                            id: "logsheets.add.button",
                            defaultMessage: "Add logsheets",
                        })}
                    </Button>
                    <LogsheetTable templateId={templateId} className="border" />
                </div>
            </Screen>
        </>
    );
};
