import { Button } from "@/components/ui/button";
import { Spinner } from "@/components/ui/spinner";
import { AlignmentEditor } from "@/modules/logsheets/alignment/components/alignment-editor";
import { useLogsheet } from "@/modules/logsheets/api";
import { baseLogsheetsPath } from "@/modules/logsheets/routes";
import { ArrowLeftIcon } from "lucide-react";
import { useIntl } from "react-intl";
import { useNavigate, useParams } from "react-router-dom";

export const LogsheetAlignmentPage = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const intl = useIntl();

    const { data: logsheet, isLoading, isError } = useLogsheet(id!);

    return (
        <div className="flex flex-col h-screen bg-background">
            <div className="border-b p-4 flex items-center justify-between bg-card">
                <div className="flex items-center gap-4">
                    <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => navigate(baseLogsheetsPath)}
                    >
                        <ArrowLeftIcon className="h-4 w-4" />
                    </Button>
                    <div>
                        <h1 className="text-lg font-semibold">
                            {intl.formatMessage({
                                id: "logsheets.alignment.title",
                                defaultMessage: "Manual Alignment",
                            })}
                        </h1>
                        {logsheet ? (
                            <p className="text-sm text-muted-foreground">
                                {logsheet.id}
                            </p>
                        ) : null}
                    </div>
                </div>
            </div>
            <div className="flex-1 overflow-hidden">
                {logsheet ? <AlignmentEditor logsheet={logsheet} /> : null}
                {isLoading ? <Spinner className="p-4" /> : null}
                {isError ? (
                    <p className="text-red-500 p-4">
                        {intl.formatMessage({
                            id: "logsheets.alignment.error",
                            defaultMessage: "Error loading logsheet",
                        })}
                    </p>
                ) : null}
            </div>
        </div>
    );
};
