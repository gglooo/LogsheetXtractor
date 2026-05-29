import { useFormatValidationWarning } from "@/modules/logsheets/proofreading/utils/validation-warning-intl";
import { useFormatValidationRulePath } from "@/modules/logsheets/proofreading/utils/validation-warning-path";
import type { RoiValidationWarningType } from "@/modules/logsheets/schema";
import type { RoiValidationConditionType } from "@/modules/rois/validation/schema";
import { TriangleAlert } from "lucide-react";
import type { ReactNode } from "react";
import { defineMessage, useIntl } from "react-intl";

type ExtractedValueValidationWarningsContentProps = {
    warnings: RoiValidationWarningType[];
    validationCondition: RoiValidationConditionType;
    headerAction?: ReactNode;
    isCollapsed?: boolean;
    collapsedSummary?: string;
    onCollapsedClick?: () => void;
};

const warningsTitleMessage = defineMessage({
    id: "proofreading.validationWarnings.title",
    defaultMessage: "Validation warnings",
});

const rulePathMessage = defineMessage({
    id: "proofreading.validationWarnings.rulePath",
    defaultMessage: "{rulePath}",
});

export const ExtractedValueValidationWarningsContent = ({
    warnings,
    validationCondition,
    headerAction,
    isCollapsed = false,
    collapsedSummary,
    onCollapsedClick,
}: ExtractedValueValidationWarningsContentProps) => {
    const intl = useIntl();
    const formatValidationWarning = useFormatValidationWarning();
    const { formatValidationRulePath, resolveNodeAtPath } =
        useFormatValidationRulePath();

    if (warnings.length === 0) {
        return null;
    }

    return (
        <div
            className={`rounded-md border border-amber-500/30 bg-amber-500/10 p-3 ${onCollapsedClick ? "cursor-pointer" : ""}`}
            onClick={onCollapsedClick}
        >
            <div
                className={`flex items-center justify-between text-amber-700 dark:text-amber-300 ${isCollapsed ? "" : "mb-2"}`}
            >
                <div className="flex items-center gap-2">
                    <TriangleAlert className="h-4 w-4" />
                    <span className="text-xs font-semibold uppercase tracking-wide">
                        {intl.formatMessage(warningsTitleMessage)}
                    </span>
                </div>
                {headerAction}
            </div>
            {isCollapsed && collapsedSummary ? (
                <div className="mt-1 text-xs text-amber-900 dark:text-amber-100">
                    {collapsedSummary}
                </div>
            ) : null}
            {!isCollapsed ? (
                <ul className="max-h-44 space-y-2 overflow-y-auto pr-1">
                    {warnings.map((warning, index) => {
                        const node = resolveNodeAtPath(
                            warning.path,
                            validationCondition,
                        );
                        const shouldDisplayRulePath =
                            node?.type === "rule" &&
                            Object.keys(node.params).length > 0;

                        return (
                            <li
                                key={`${warning.code}-${warning.path}-${index}`}
                                className="text-sm text-amber-900 dark:text-amber-100"
                            >
                                <div>{formatValidationWarning(warning)}</div>
                                {shouldDisplayRulePath && (
                                    <div className="text-xs text-amber-800/80 dark:text-amber-200/80">
                                        {intl.formatMessage(rulePathMessage, {
                                            rulePath: formatValidationRulePath(
                                                warning.path,
                                                validationCondition,
                                            ),
                                        })}
                                    </div>
                                )}
                            </li>
                        );
                    })}
                </ul>
            ) : null}
        </div>
    );
};
