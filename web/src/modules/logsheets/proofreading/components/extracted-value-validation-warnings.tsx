import { Button } from "@/components/ui/button";
import { ExtractedValueValidationWarningsContent } from "@/modules/logsheets/proofreading/components/extracted-value-validation-warnings-content";
import type { RoiValidationWarningType } from "@/modules/logsheets/schema";
import type { RoiValidationConditionType } from "@/modules/rois/validation/schema";
import { ChevronDown, ChevronUp } from "lucide-react";
import { useEffect, useRef, useState } from "react";
import { useFormContext } from "react-hook-form";
import { defineMessage, useIntl } from "react-intl";

type ExtractedValueValidationWarningsProps = {
    warnings: RoiValidationWarningType[];
    validationCondition: RoiValidationConditionType;
    initialCorrectedValue: string | number | null;
};

const collapsedSummaryMessage = defineMessage({
    id: "proofreading.validationWarnings.collapsedSummary",
    defaultMessage:
        "{count, plural, one {# warning from original extraction} other {# warnings from original extraction}}",
});

const showWarningsMessage = defineMessage({
    id: "proofreading.validationWarnings.show",
    defaultMessage: "Show warnings",
});

const minimizeWarningsMessage = defineMessage({
    id: "proofreading.validationWarnings.minimize",
    defaultMessage: "Hide warnings",
});

const normalizeComparableValue = (value: unknown): string => {
    if (value === null || value === undefined) {
        return "";
    }

    if (typeof value === "number") {
        return Number.isNaN(value) ? "" : `${value}`;
    }

    if (typeof value === "boolean") {
        return value ? "true" : "false";
    }

    return `${value}`;
};

export const ExtractedValueValidationWarnings = ({
    warnings,
    validationCondition,
    initialCorrectedValue,
}: ExtractedValueValidationWarningsProps) => {
    const intl = useIntl();
    const { watch } = useFormContext();
    const hasAutoCollapsedRef = useRef(false);
    const [isCollapsed, setIsCollapsed] = useState(false);

    useEffect(() => {
        const initialComparable = normalizeComparableValue(
            initialCorrectedValue,
        );

        const subscription = watch((values, { name }) => {
            if (name !== "correctedValue" || hasAutoCollapsedRef.current) {
                return;
            }

            const currentComparable = normalizeComparableValue(
                values.correctedValue,
            );

            if (currentComparable === initialComparable) {
                return;
            }

            hasAutoCollapsedRef.current = true;
            setIsCollapsed(true);
        });

        return () => subscription.unsubscribe();
    }, [initialCorrectedValue, watch]);

    if (warnings.length === 0) {
        return null;
    }

    return (
        <ExtractedValueValidationWarningsContent
            warnings={warnings}
            validationCondition={validationCondition}
            isCollapsed={isCollapsed}
            onCollapsedClick={() => setIsCollapsed((prev) => !prev)}
            collapsedSummary={
                isCollapsed
                    ? intl.formatMessage(collapsedSummaryMessage, {
                          count: warnings.length,
                      })
                    : undefined
            }
            headerAction={
                <Button
                    type="button"
                    variant="ghost"
                    size="icon"
                    onClick={(event) => {
                        event.stopPropagation();
                        setIsCollapsed((prev) => !prev);
                    }}
                    aria-label={intl.formatMessage(
                        isCollapsed
                            ? showWarningsMessage
                            : minimizeWarningsMessage,
                    )}
                    className="h-6 w-6 text-amber-700 hover:bg-amber-500/10 hover:text-amber-700 dark:text-amber-300 dark:hover:bg-amber-400/10 dark:hover:text-amber-300"
                >
                    {isCollapsed ? (
                        <ChevronDown className="h-4 w-4" />
                    ) : (
                        <ChevronUp className="h-4 w-4" />
                    )}
                </Button>
            }
        />
    );
};
