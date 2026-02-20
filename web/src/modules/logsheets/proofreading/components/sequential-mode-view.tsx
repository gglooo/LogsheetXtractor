import { buttonVariants } from "@/components/ui/button";
import { Spinner } from "@/components/ui/spinner";
import { cn } from "@/lib/utils";
import { useLogsheet } from "@/modules/logsheets/api";
import { useNextLogsheetUnverifiedExtractedValues } from "@/modules/logsheets/proofreading/api";
import { GamifiedCard } from "@/modules/logsheets/proofreading/components/gamified-card";
import { GamifiedComplete } from "@/modules/logsheets/proofreading/components/gamified-complete";
import { baseLogsheetsPath } from "@/modules/logsheets/routes";
import { useQueryClient } from "@tanstack/react-query";
import { ArrowRightIcon } from "lucide-react";
import { useCallback, useState } from "react";
import { useIntl } from "react-intl";
import { Link } from "react-router-dom";

type SequentialModeViewProps = {
    onVerifiedCountChange: (delta: number) => void;
};

const LogsheetBanner = ({ logsheetId }: { logsheetId: string }) => {
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

export const SequentialModeView = ({
    onVerifiedCountChange,
}: SequentialModeViewProps) => {
    const [queryEnabled, setQueryEnabled] = useState(true);
    const { data, isLoading, isError } =
        useNextLogsheetUnverifiedExtractedValues(queryEnabled);
    const queryClient = useQueryClient();

    const handleNext = useCallback(() => {
        onVerifiedCountChange(1);
        setQueryEnabled(false);
        queryClient
            .invalidateQueries({
                queryKey: ["extracted-values", "unverified", "next-logsheet"],
            })
            .then(() => setQueryEnabled(true));
    }, [queryClient, onVerifiedCountChange]);

    if (isLoading) {
        return (
            <div className="flex items-center justify-center flex-1">
                <Spinner />
            </div>
        );
    }

    if (isError) {
        return (
            <div className="flex items-center justify-center flex-1 text-destructive">
                Failed to load value
            </div>
        );
    }

    if (data === null || data === undefined) {
        return <GamifiedComplete />;
    }

    return (
        <div className="flex flex-col flex-1 overflow-y-auto p-6 items-center justify-center">
            <div className="w-full">
                <LogsheetBanner logsheetId={data.logsheetId} />
                <GamifiedCard
                    extractedValue={data}
                    isFetching={!queryEnabled}
                    onNext={handleNext}
                />
            </div>
        </div>
    );
};
