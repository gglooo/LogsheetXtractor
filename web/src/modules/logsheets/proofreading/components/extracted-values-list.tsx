import { cn } from "@/lib/utils";
import type { ExtractedValueType } from "@/modules/logsheets/schema";
import { forwardRef, useImperativeHandle, useRef } from "react";
import { Virtuoso, type VirtuosoHandle } from "react-virtuoso";
import { ExtractedValueCard } from "./extracted-value-card";

type ExtractedValuesListProps = {
    extractedValues: ExtractedValueType[];
    className?: string;
};

export type ExtractedValuesListHandle = {
    scrollToRoi: (roiId: string) => void;
};

export const ExtractedValuesList = forwardRef<
    ExtractedValuesListHandle,
    ExtractedValuesListProps
>(({ extractedValues, className }, ref) => {
    const virtuosoRef = useRef<VirtuosoHandle>(null);

    useImperativeHandle(ref, () => ({
        scrollToRoi: (roiId: string) => {
            const index = extractedValues.findIndex(
                (val) => val.roiId === roiId
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

    if (extractedValues.length === 0) {
        return (
            <div
                className={cn(
                    "flex items-center justify-center h-full p-4",
                    className
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
            itemContent={(_, value) => (
                <div className="pb-4 pr-2 pl-2" key={value.id}>
                    <ExtractedValueCard extractedValue={value} />
                </div>
            )}
        />
    );
});
