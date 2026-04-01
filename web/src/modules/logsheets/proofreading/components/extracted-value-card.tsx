import { Card, CardContent } from "@/components/ui/card";
import { cn } from "@/lib/utils";
import { ExtractedValueForm } from "@/modules/logsheets/proofreading/components/extracted-value-form";
import { ExtractedValueImage } from "@/modules/logsheets/proofreading/components/extracted-value-image";
import type { ExtractedValueType } from "@/modules/logsheets/schema";
import type { RoiValidationConditionType } from "@/modules/rois/validation/schema";
import { memo } from "react";

type ExtractedValueCardProps = {
    extractedValue: ExtractedValueType;
    validationCondition: RoiValidationConditionType;
    isSelected: boolean;
    onSelect: (roiId: string) => void;
    onVerified?: (roiId: string) => void;
};

export const ExtractedValueCard = memo(
    ({
        extractedValue,
        validationCondition,
        isSelected,
        onSelect,
        onVerified,
    }: ExtractedValueCardProps) => {
        return (
            <Card
                onClick={() => onSelect(extractedValue.roiId)}
                data-proofreading-roi-id={extractedValue.roiId}
                className={cn(
                    "cursor-pointer transition-all duration-200 border-2",
                    isSelected
                        ? "border-primary ring-1 ring-primary"
                        : "border-border",
                )}
            >
                <CardContent className="p-4 flex gap-4">
                    <ExtractedValueImage id={extractedValue.id} />
                    <div className="flex-1 gap-4 flex flex-col">
                        <div className="font-medium text-sm text-muted-foreground">
                            {extractedValue.variableName}
                        </div>
                        <ExtractedValueForm
                            extractedValue={extractedValue}
                            validationCondition={validationCondition}
                            onVerified={() => onVerified?.(extractedValue.roiId)}
                        />
                    </div>
                    <div className="flex flex-col justify-end"></div>
                </CardContent>
            </Card>
        );
    },
);
