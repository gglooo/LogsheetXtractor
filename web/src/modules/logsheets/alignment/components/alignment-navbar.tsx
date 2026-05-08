import { Button } from "@/components/ui/button";
import { useNavigateUp } from "@/hooks/use-navigate-up";
import type { LogsheetType } from "@/modules/logsheets/schema";
import { ArrowLeftIcon } from "lucide-react";
import { useIntl } from "react-intl";

export const AlignmentNavbar = ({ logsheet }: { logsheet?: LogsheetType }) => {
    const navigateUp = useNavigateUp();
    const intl = useIntl();

    return (
        <div className="border-b p-4 flex items-center justify-between bg-card">
            <div className="flex items-center gap-4">
                <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => navigateUp()}
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
                    {logsheet ? (
                        <p className="text-sm text-muted-foreground">
                            {logsheet.file.fileName}
                        </p>
                    ) : null}
                </div>
            </div>
        </div>
    );
};
