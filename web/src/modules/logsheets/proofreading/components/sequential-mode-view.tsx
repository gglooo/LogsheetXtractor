import { Spinner } from "@/components/ui/spinner";
import { useNextLogsheetUnverifiedExtractedValues } from "@/modules/logsheets/proofreading/api";
import { GamifiedCard } from "@/modules/logsheets/proofreading/components/gamified-card";
import { GamifiedComplete } from "@/modules/logsheets/proofreading/components/gamified-complete";
import { LogsheetBanner } from "@/modules/logsheets/proofreading/components/logsheet-banner";
import { useQueryClient } from "@tanstack/react-query";
import { useCallback, useState } from "react";

type SequentialModeViewProps = {
    onVerifiedCountChange: (delta: number) => void;
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
