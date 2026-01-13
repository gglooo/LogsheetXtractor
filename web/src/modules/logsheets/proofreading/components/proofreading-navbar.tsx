import { Button } from "@/components/ui/button";
import { useCompleteProofreadingMutation } from "@/modules/logsheets/proofreading/api";
import { useIntl } from "react-intl";
import { toast } from "sonner";

export const ProofreadingNavbar = ({
    toReviewCount,
    logsheetId,
}: {
    toReviewCount: number;
    logsheetId: string;
}) => {
    const intl = useIntl();

    const completeProofReadingMutation =
        useCompleteProofreadingMutation(logsheetId);

    const handleCompleteProofreading = async () => {
        try {
            await completeProofReadingMutation.mutateAsync();
            toast.success(
                intl.formatMessage({
                    id: "proofreading.complete.success",
                    defaultMessage: "Proofreading completed successfully.",
                })
            );
        } catch (error) {
            console.error("Error completing proofreading:", error);
            toast.error(
                intl.formatMessage({
                    id: "proofreading.complete.error",
                    defaultMessage:
                        "An error occurred while completing proofreading.",
                })
            );
        }
    };

    return (
        <div className="border-b h-14 flex items-center justify-between px-4 bg-background shrink-0">
            <h1 className="font-semibold text-lg">
                {intl.formatMessage({
                    id: "proofreading.title",
                    defaultMessage: "Proofreading",
                })}
            </h1>
            <div className="flex items-center gap-2">
                <Button
                    onClick={handleCompleteProofreading}
                    disabled={toReviewCount !== 0}
                >
                    {intl.formatMessage({
                        id: "proofreading.verify",
                        defaultMessage: "Complete proofreading",
                    })}
                </Button>
            </div>
        </div>
    );
};
