import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { useLogsheet } from "@/modules/logsheets/api";
import { baseLogsheetsPath } from "@/modules/logsheets/routes";
import { ArrowRightIcon } from "lucide-react";
import { useIntl } from "react-intl";
import { Link } from "react-router-dom";

export const LogsheetBanner = ({ logsheetId }: { logsheetId: string }) => {
    const intl = useIntl();
    const { data: logsheet } = useLogsheet(logsheetId);

    if (!logsheet) return null;

    return (
        <div className="w-full max-w-xl mx-auto mb-6 flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 p-4 bg-muted/50 rounded-lg border border-border">
            <div className="flex flex-col">
                <span className="text-sm font-medium text-foreground">
                    {logsheet.file.fileName}
                </span>
                <span className="text-xs text-muted-foreground">
                    {intl.formatMessage({
                        id: "gamified.sequential.currentLogsheet",
                        defaultMessage: "Currently proofreading",
                    })}
                    : {logsheet.template.name ?? "Template"}
                </span>
            </div>
            <Link
                to={`/templates/${logsheet.template.id}${baseLogsheetsPath}/${logsheet.id}/proofread`}
                className={cn(
                    buttonVariants({ variant: "ghost", size: "sm" }),
                    "text-xs gap-1",
                )}
            >
                {intl.formatMessage({
                    id: "gamified.sequential.goToFull",
                    defaultMessage: "Go to full proofreading",
                })}
                <ArrowRightIcon className="w-3 h-3" />
            </Link>
        </div>
    );
};
