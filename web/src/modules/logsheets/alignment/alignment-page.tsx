import { Spinner } from "@/components/ui/spinner";
import { AlignmentEditor } from "@/modules/logsheets/alignment/components/alignment-editor";
import { AlignmentNavbar } from "@/modules/logsheets/alignment/components/alignment-navbar";
import { useLogsheet } from "@/modules/logsheets/api";
import { useIntl } from "react-intl";
import { useParams } from "react-router-dom";

export const LogsheetAlignmentPage = () => {
    const { id } = useParams<{ id: string }>();
    const intl = useIntl();

    const { data: logsheet, isLoading, isError } = useLogsheet(id!);

    return (
        <div className="flex flex-col h-screen bg-background">
            <AlignmentNavbar logsheetId={logsheet?.id} />
            <div className="flex-1 overflow-hidden">
                {logsheet ? <AlignmentEditor logsheet={logsheet} /> : null}
                {isLoading ? <Spinner className="p-4" /> : null}
                {isError ? (
                    <p className="text-destructive p-4">
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
