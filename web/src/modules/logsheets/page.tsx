import { Screen } from "@/components/screen";
import { LogsheetsNavbar } from "@/modules/logsheets/components/navbar";
import { LogsheetTable } from "@/modules/logsheets/table/logsheet-table";
import { useParams } from "react-router-dom";

export const LogsheetsPage = () => {
    const { templateId } = useParams<{ templateId: string }>();

    if (!templateId) {
        return <div className="p-4">No template ID provided.</div>;
    }

    return (
        <>
            <LogsheetsNavbar />
            <Screen className="bg-background">
                <div className="flex flex-col flex-1 px-4">
                    <LogsheetTable templateId={templateId} className="border" />
                </div>
            </Screen>
        </>
    );
};
