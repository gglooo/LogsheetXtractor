import { Spinner } from "@/components/ui/spinner";
import { useRandomUnverifiedExtractedValue } from "@/modules/logsheets/proofreading/api";
import { GamifiedCard } from "@/modules/logsheets/proofreading/components/gamified-card";
import { GamifiedComplete } from "@/modules/logsheets/proofreading/components/gamified-complete";
import { useQueryClient } from "@tanstack/react-query";
import { useCallback } from "react";
import { useIntl } from "react-intl";

type RandomModeViewProps = {
    onVerifiedCountChange: (delta: number) => void;
};

export const RandomModeView = ({
    onVerifiedCountChange,
}: RandomModeViewProps) => {
    const intl = useIntl();

    const { data, isLoading, isError, isFetching } =
        useRandomUnverifiedExtractedValue();

    const queryClient = useQueryClient();

    const handleNext = useCallback(async () => {
        onVerifiedCountChange(1);

        await queryClient.invalidateQueries({
            queryKey: ["extracted-values", "unverified", "random"],
        });
    }, [queryClient, onVerifiedCountChange]);

    const handleSkip = useCallback(async () => {
        await queryClient.invalidateQueries({
            queryKey: ["extracted-values", "unverified", "random"],
        });
    }, [queryClient]);

    if (isLoading) {
        return (
            <div className="flex items-center justify-center flex-1">
                <Spinner />
            </div>
        );
    }

    if (isError) {
        return (
            <div className="flex flex-1 items-center justify-center text-destructive">
                {intl.formatMessage({
                    id: "proofreading.extractedValue.verify.error",
                    defaultMessage: "Failed to verify value",
                })}
            </div>
        );
    }

    if (data === null || data === undefined) {
        return <GamifiedComplete />;
    }

    return (
        <div className="flex flex-1 flex-col items-center justify-center overflow-y-auto p-6">
            <GamifiedCard
                extractedValue={data}
                isFetching={isFetching}
                onNext={handleNext}
                onSkip={handleSkip}
            />
        </div>
    );
};
