import { cn } from "@/lib/utils";
import type { ExtractedValueType } from "@/modules/logsheets/schema";
import type { RoiValidationConditionType } from "@/modules/rois/validation/schema";
import { useSelectedRois } from "@/modules/template-editor/hooks/use-selected-rois";
import { forwardRef, useCallback, useImperativeHandle, useRef } from "react";
import { Virtuoso, type VirtuosoHandle } from "react-virtuoso";
import { ExtractedValueCard } from "./extracted-value-card";

type ExtractedValuesListProps = {
    extractedValues: ExtractedValueType[];
    validationConditionsByRoiId: Record<string, RoiValidationConditionType>;
    className?: string;
    onRoiClick?: (roiId: string) => void;
};

export type ExtractedValuesListHandle = {
    scrollToRoi: (roiId: string) => void;
};

export const ExtractedValuesList = forwardRef<
    ExtractedValuesListHandle,
    ExtractedValuesListProps
>(
    ({ extractedValues, validationConditionsByRoiId, className, onRoiClick }, ref) => {
    const virtuosoRef = useRef<VirtuosoHandle>(null);
    const { setSelectedRoiIds, selectedRoiIds } = useSelectedRois();

    useImperativeHandle(ref, () => ({
        scrollToRoi: (roiId: string) => {
            const index = extractedValues.findIndex(
                (val) => val.roiId === roiId,
            );

            if (index !== -1 && virtuosoRef.current) {
                virtuosoRef.current.scrollToIndex({
                    index,
                    align: "center",
                    behavior: "auto",
                });
            }
        },
    }));

    const handleSelect = useCallback(
        (roiId: string) => {
            setSelectedRoiIds([roiId]);
            onRoiClick?.(roiId);
        },
        [setSelectedRoiIds, onRoiClick],
    );

    if (extractedValues.length === 0) {
        return (
            <div
                className={cn(
                    "flex items-center justify-center h-full p-4",
                    className,
                )}
            >
                No extracted values to process
            </div>
        );
    }

    return (
        <Virtuoso
            ref={virtuosoRef}
            data={extractedValues}
            className={cn("h-full", className)}
            context={{ selectedRoiIds, onSelect: handleSelect }}
            itemContent={(_, value, { selectedRoiIds, onSelect }) => (
                <div className="pb-4 pr-2 pl-2" key={value.id}>
                    <ExtractedValueCard
                        extractedValue={value}
                        validationCondition={
                            validationConditionsByRoiId[value.roiId] ?? null
                        }
                        isSelected={selectedRoiIds.includes(value.roiId)}
                        onSelect={onSelect}
                    />
                </div>
            )}
        />
    );
},
);
