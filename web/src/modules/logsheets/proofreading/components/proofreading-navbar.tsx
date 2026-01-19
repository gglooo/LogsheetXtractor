import { NavbarContainer } from "@/components/navbar-container";
import { Button } from "@/components/ui/button";
import { Spinner } from "@/components/ui/spinner";
import { ResetProofreadingAction } from "@/modules/logsheets/proofreading/actions/reset-proofreading-action";
import { useCompleteProofreadingMutation } from "@/modules/logsheets/proofreading/api";
import { ArrowLeft } from "lucide-react";
import { useIntl } from "react-intl";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";

export const ProofreadingNavbar = ({
    toReviewCount,
    logsheetId,
}: {
    toReviewCount: number;
    logsheetId: string;
}) => {
    const intl = useIntl();

    const navigate = useNavigate();

    const completeProofReadingMutation =
        useCompleteProofreadingMutation(logsheetId);

    const handleCompleteProofreading = async () => {
        try {
            await completeProofReadingMutation.mutateAsync();
            navigate(-1);

            toast.success(
                intl.formatMessage({
                    id: "proofreading.complete.success",
                    defaultMessage: "Proofreading completed successfully.",
                }),
            );
        } catch (error) {
            console.error("Error completing proofreading:", error);
            toast.error(
                intl.formatMessage({
                    id: "proofreading.complete.error",
                    defaultMessage:
                        "An error occurred while completing proofreading.",
                }),
            );
        }
    };

    return (
        <NavbarContainer>
            <div className="flex flex-row items-center gap-4">
                <ArrowLeft
                    className="cursor-pointer"
                    onClick={() => navigate(-1)}
                />
                <div className="flex justify-between w-full">
                    <div className="p-4 text-lg font-bold">
                        {intl.formatMessage({
                            id: "logsheets.title",
                            defaultMessage: "Logsheets",
                        })}
                    </div>
                    <div className="flex items-center gap-2 p-4"></div>
                </div>
            </div>
            <div className="flex items-center gap-2">
                <ResetProofreadingAction logsheetId={logsheetId} />
                <Button
                    onClick={handleCompleteProofreading}
                    disabled={
                        toReviewCount !== 0 ||
                        completeProofReadingMutation.isPending
                    }
                >
                    {completeProofReadingMutation.isPending ? (
                        <Spinner />
                    ) : (
                        intl.formatMessage({
                            id: "proofreading.verify",
                            defaultMessage: "Complete proofreading",
                        })
                    )}
                </Button>
            </div>
        </NavbarContainer>
    );
};
