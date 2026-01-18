import { Button } from "@/components/ui/button";
import { ArrowLeftIcon } from "lucide-react";
import { useIntl } from "react-intl";
import { useNavigate } from "react-router-dom";

export const AlignmentNavbar = ({ logsheetId }: { logsheetId?: string }) => {
    const navigate = useNavigate();
    const intl = useIntl();

    return (
        <div className="border-b p-4 flex items-center justify-between bg-card">
            <div className="flex items-center gap-4">
                <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => navigate(-1)}
                >
                    <ArrowLeftIcon className="h-4 w-4" />
                </Button>
                <div>
                    <h1 className="text-lg font-semibold">
                        {intl.formatMessage({
                            id: "logsheets.alignment.title",
                            defaultMessage: "Manual alignment",
                        })}
                    </h1>
                    {logsheetId ? (
                        <p className="text-sm text-muted-foreground">
                            {logsheetId}
                        </p>
                    ) : null}
                </div>
            </div>
        </div>
    );
};
