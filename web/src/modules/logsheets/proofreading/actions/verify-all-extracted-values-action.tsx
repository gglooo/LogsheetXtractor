import { Button } from "@/components/ui/button";
import { useVerifyExtractedValuesMutation } from "@/modules/logsheets/proofreading/api";
import type { ExtractedValueType } from "@/modules/logsheets/schema";
import { CheckCheck, Loader2 } from "lucide-react";
import { useIntl } from "react-intl";
import { toast } from "sonner";

export const VerifyAllExtractedValuesAction = ({
    logsheetId,
    unverifiedExtractedValues,
}: {
    logsheetId: string;
    unverifiedExtractedValues: ExtractedValueType[];
}) => {
    const intl = useIntl();
    const verifyExtractedValuesMutation =
        useVerifyExtractedValuesMutation(logsheetId);

    const handleVerifyAll = async () => {
        try {
            await verifyExtractedValuesMutation.mutateAsync({
                extractedValueIds: unverifiedExtractedValues.map((v) => v.id),
            });
            toast.success(
                intl.formatMessage({
                    id: "logsheets.proofreading.verifyAll.success",
                    defaultMessage: "All values verified successfully",
                }),
            );
        } catch (error) {
            console.error("Error verifying all values:", error);
            toast.error(
                intl.formatMessage({
                    id: "logsheets.proofreading.verifyAll.error",
                    defaultMessage: "Failed to verify all values",
                }),
            );
        }
    };

    if (unverifiedExtractedValues.length === 0) {
        return null;
    }

    return (
        <div className="p-4 border-b bg-muted/10">
            <Button
                className="w-full"
                size="sm"
                onClick={handleVerifyAll}
                disabled={verifyExtractedValuesMutation.isPending}
            >
                {verifyExtractedValuesMutation.isPending ? (
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                ) : (
                    <CheckCheck className="mr-2 h-4 w-4" />
                )}
                {intl.formatMessage({
                    id: "logsheets.proofreading.verifyAll",
                    defaultMessage: "Verify all",
                })}
            </Button>
        </div>
    );
};
