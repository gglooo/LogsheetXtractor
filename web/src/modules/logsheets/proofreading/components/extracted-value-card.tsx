import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Spinner } from "@/components/ui/spinner";
import { cn } from "@/lib/utils";
import {
    useExtractedValueImage,
    useVerifyExtractedValueMutation,
} from "@/modules/logsheets/proofreading/api";
import type { ExtractedValueType } from "@/modules/logsheets/schema";
import { useSelectedRois } from "@/modules/template-editor/hooks/use-selected-rois";
import { Check } from "lucide-react";
import { useIntl } from "react-intl";
import { toast } from "sonner";

type ExtractedValueCardProps = {
    extractedValue: ExtractedValueType;
};

const ExtractedValueImage = ({ id }: { id: string }) => {
    const { data, isLoading, isError } = useExtractedValueImage(id);

    if (isLoading) {
        return <Spinner />;
    }

    if (isError || !data) {
        return <div>Error loading image</div>;
    }

    return (
        <img
            src={URL.createObjectURL(new Blob([data.bytes]))}
            alt="Extracted Value"
            className="max-w-full max-h-full object-contain"
        />
    );
};

export const EXTRACTED_VALUE_CARD_ID_PREFIX = "ev-card-";

export const ExtractedValueCard = ({
    extractedValue,
}: ExtractedValueCardProps) => {
    const intl = useIntl();

    const verifyExtractedValueMutation = useVerifyExtractedValueMutation(
        extractedValue.logsheetId
    );

    const { setSelectedRoiIds, isSelectedRoi } = useSelectedRois();
    const associatedRoiId = extractedValue.roiId;
    const isSelected = isSelectedRoi(associatedRoiId);

    const handleVerifyClick = async () => {
        try {
            await verifyExtractedValueMutation.mutateAsync({
                extractedValueId: extractedValue.id,
            });
            toast.success(
                intl.formatMessage({
                    id: "proofreading.verifySuccess",
                    defaultMessage: "Extracted value verified.",
                })
            );
        } catch (error) {
            console.error("Error verifying extracted value:", error);
            toast.error(
                intl.formatMessage({
                    id: "proofreading.verifyError",
                    defaultMessage: "Failed to verify extracted value.",
                })
            );
        }
    };

    const handleCardClick = () => {
        if (!associatedRoiId) {
            return;
        }
        setSelectedRoiIds([associatedRoiId]);
    };

    return (
        <Card
            id={`${EXTRACTED_VALUE_CARD_ID_PREFIX}${extractedValue.roiId}`}
            onClick={handleCardClick}
            className={cn(
                "cursor-pointer transition-all duration-200 border-2",
                isSelected
                    ? "border-primary ring-1 ring-primary"
                    : "border-border"
            )}
        >
            <CardContent className="p-4 flex gap-4">
                <div className="w-25 h-25 bg-muted rounded-md flex items-center justify-center text-muted-foreground text-xs">
                    <ExtractedValueImage id={extractedValue.id} />
                </div>
                <div className="flex-1 space-y-2">
                    <div className="font-medium text-sm text-muted-foreground">
                        {extractedValue.variableName}
                    </div>
                    <div className="flex flex-row gap-4 items-end flex-1">
                        <div className="space-y-1 flex-1">
                            <label className="text-xs font-medium">
                                {intl.formatMessage({
                                    id: "proofreading.originalValue",
                                    defaultMessage: "Original",
                                })}
                            </label>
                            <Input
                                readOnly
                                value={extractedValue.value}
                                className="bg-muted/50"
                            />
                        </div>
                        <div className="space-y-1 flex-1">
                            <label className="text-xs font-medium">
                                {intl.formatMessage({
                                    id: "proofreading.correctedValue",
                                    defaultMessage: "Corrected",
                                })}
                            </label>
                            <Input
                                placeholder={extractedValue.value}
                                defaultValue={
                                    extractedValue.correctedValue ??
                                    extractedValue.value
                                }
                            />
                        </div>
                        <Button
                            size="icon"
                            variant={
                                extractedValue.status === "Verified"
                                    ? "default"
                                    : "outline"
                            }
                            onClick={handleVerifyClick}
                            disabled={verifyExtractedValueMutation.isPending}
                            className="hover:bg-green-500/10 hover:border-green-500 focus:ring-green-500"
                        >
                            {verifyExtractedValueMutation.isPending ? (
                                <Spinner />
                            ) : (
                                <Check className="h-4 w-4" />
                            )}
                        </Button>
                    </div>
                </div>
                <div className="flex flex-col justify-end"></div>
            </CardContent>
        </Card>
    );
};
