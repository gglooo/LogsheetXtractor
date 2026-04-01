import { cn } from "@/lib/utils";
import type { ExtractedValueType } from "@/modules/logsheets/schema";
import type { RoiValidationConditionType } from "@/modules/rois/validation/schema";
import { useSelectedRois } from "@/modules/template-editor/hooks/use-selected-rois";
import {
    forwardRef,
    useCallback,
    useEffect,
    useImperativeHandle,
    useRef,
} from "react";
import { useIntl } from "react-intl";
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
    (
        { extractedValues, validationConditionsByRoiId, className, onRoiClick },
        ref,
    ) => {
        const virtuosoRef = useRef<VirtuosoHandle>(null);
        const pendingFocusRoiIdRef = useRef<string | null>(null);
        const { setSelectedRoiIds, selectedRoiIds } = useSelectedRois();

        const intl = useIntl();

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

        const focusCorrectedFieldByRoiId = useCallback((roiId: string) => {
            let attempts = 8;

            const tryFocus = () => {
                const card = document.querySelector<HTMLElement>(
                    `[data-proofreading-roi-id="${roiId}"]`,
                );
                const correctedValueField = card?.querySelector<
                    HTMLInputElement | HTMLSelectElement
                >(
                    'input[name="correctedValue"], select[name="correctedValue"]',
                );

                if (!correctedValueField) {
                    if (attempts > 0) {
                        attempts -= 1;
                        requestAnimationFrame(tryFocus);
                    }
                    return;
                }

                correctedValueField.focus();
                if (correctedValueField instanceof HTMLInputElement) {
                    correctedValueField.select();
                }
            };

            tryFocus();
        }, []);

        const handleValueVerified = useCallback(
            (verifiedRoiId: string) => {
                const verifiedValueIndex = extractedValues.findIndex(
                    (value) => value.roiId === verifiedRoiId,
                );

                if (verifiedValueIndex === -1) {
                    return;
                }

                if (extractedValues.length === 0) {
                    pendingFocusRoiIdRef.current = null;
                    return;
                }

                const nextValueIndex =
                    (verifiedValueIndex + 1) % extractedValues.length;

                pendingFocusRoiIdRef.current =
                    extractedValues[nextValueIndex]?.roiId ?? null;
            },
            [extractedValues],
        );

        const focusPendingNextCardAfterVerification = useCallback(() => {
            const pendingFocusRoiId = pendingFocusRoiIdRef.current;

            if (!pendingFocusRoiId) {
                return;
            }

            const pendingFocusIndex = extractedValues.findIndex(
                (value) => value.roiId === pendingFocusRoiId,
            );

            if (pendingFocusIndex === -1) {
                pendingFocusRoiIdRef.current = null;
                return;
            }

            virtuosoRef.current?.scrollToIndex({
                index: pendingFocusIndex,
                align: "center",
                behavior: "auto",
            });
            handleSelect(pendingFocusRoiId);
            requestAnimationFrame(() =>
                focusCorrectedFieldByRoiId(pendingFocusRoiId),
            );

            pendingFocusRoiIdRef.current = null;
        }, [extractedValues, focusCorrectedFieldByRoiId, handleSelect]);

        useEffect(() => {
            focusPendingNextCardAfterVerification();
        }, [focusPendingNextCardAfterVerification]);

        if (extractedValues.length === 0) {
            return (
                <div
                    className={cn(
                        "flex items-center justify-center h-full p-4",
                        className,
                    )}
                >
                    {intl.formatMessage({
                        id: "proofreading.noExtractedValues",
                        defaultMessage: "No extracted values found.",
                    })}
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
                            onVerified={handleValueVerified}
                        />
                    </div>
                )}
            />
        );
    },
);
