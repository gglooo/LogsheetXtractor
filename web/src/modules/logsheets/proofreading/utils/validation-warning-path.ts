import type {
    RoiValidationConditionNodeType,
    RoiValidationConditionType,
} from "@/modules/rois/validation/schema";
import { defineMessages, useIntl } from "react-intl";

const RULE_MESSAGES = defineMessages({
    "common.requiredNonEmpty": {
        id: "proofreading.validationWarnings.rule.common.requiredNonEmpty",
        defaultMessage: "Required value",
    },
    "number.decimalScaleMax": {
        id: "proofreading.validationWarnings.rule.number.decimalScaleMax",
        defaultMessage: "Maximum decimal places: {max}",
    },
    "number.integerOnly": {
        id: "proofreading.validationWarnings.rule.number.integerOnly",
        defaultMessage: "Integer only",
    },
    "number.notInSet": {
        id: "proofreading.validationWarnings.rule.number.notInSet",
        defaultMessage: "Forbidden values: {values}",
    },
    "number.range": {
        id: "proofreading.validationWarnings.rule.number.range",
        defaultMessage: "Range: {range}",
    },
    "text.allowedValues": {
        id: "proofreading.validationWarnings.rule.text.allowedValues",
        defaultMessage: "Allowed values: {values}",
    },
    "text.maxLength": {
        id: "proofreading.validationWarnings.rule.text.maxLength",
        defaultMessage: "Maximum length: {max}",
    },
    "text.minLength": {
        id: "proofreading.validationWarnings.rule.text.minLength",
        defaultMessage: "Minimum length: {min}",
    },
    "text.notRegex": {
        id: "proofreading.validationWarnings.rule.text.notRegex",
        defaultMessage: "Must not match pattern: {pattern}",
    },
    "text.prefix": {
        id: "proofreading.validationWarnings.rule.text.prefix",
        defaultMessage: "Required prefix: {prefix}",
    },
    "text.regex": {
        id: "proofreading.validationWarnings.rule.text.regex",
        defaultMessage: "Required pattern: {pattern}",
    },
    "text.suffix": {
        id: "proofreading.validationWarnings.rule.text.suffix",
        defaultMessage: "Required suffix: {suffix}",
    },
    group: {
        id: "proofreading.validationWarnings.rule.group",
        defaultMessage: "Condition group ({operator})",
    },
    unknownRule: {
        id: "proofreading.validationWarnings.rule.unknownRule",
        defaultMessage: "Rule: {ruleType}",
    },
    unknownPath: {
        id: "proofreading.validationWarnings.rule.unknownPath",
        defaultMessage: "Rule path: {path}",
    },
    undefinedBound: {
        id: "proofreading.validationWarnings.rule.range.undefinedBound",
        defaultMessage: "unbounded",
    },
    rangeSeparator: {
        id: "proofreading.validationWarnings.rule.range.separator",
        defaultMessage: ", ",
    },
});

const extractChildIndexes = (path: string) => {
    const matches = path.matchAll(/children\[(\d+)\]/g);
    return Array.from(matches, (match) => Number.parseInt(match[1], 10));
};

const toStringArray = (value: unknown): string[] => {
    if (!Array.isArray(value)) {
        return [];
    }

    return value.map((item) => String(item));
};

const toNumber = (value: unknown): number | undefined => {
    return typeof value === "number" ? value : undefined;
};

const toString = (value: unknown): string | undefined => {
    return typeof value === "string" ? value : undefined;
};

const toBoolean = (value: unknown): boolean | undefined => {
    return typeof value === "boolean" ? value : undefined;
};

export const useFormatValidationRulePath = () => {
    const intl = useIntl();

    const resolveNodeAtPath = (
        path: string,
        validationCondition: RoiValidationConditionType,
    ): RoiValidationConditionNodeType | null => {
        if (!validationCondition) {
            return null;
        }

        const indexes = extractChildIndexes(path);
        let currentNode: RoiValidationConditionNodeType = validationCondition;

        for (const childIndex of indexes) {
            if (currentNode.type !== "group") {
                return null;
            }

            const childNode: RoiValidationConditionNodeType | undefined =
                currentNode.children[childIndex];
            if (!childNode) {
                return null;
            }

            currentNode = childNode;
        }

        return currentNode;
    };

    const formatRange = (params: Record<string, unknown>) => {
        const min = toNumber(params.min);
        const max = toNumber(params.max);
        const inclusiveMin = toBoolean(params.inclusiveMin) ?? true;
        const inclusiveMax = toBoolean(params.inclusiveMax) ?? true;
        const undefinedBound = intl.formatMessage(RULE_MESSAGES.undefinedBound);

        const minPart =
            min === undefined
                ? undefinedBound
                : `${inclusiveMin ? ">=" : ">"} ${intl.formatNumber(min)}`;
        const maxPart =
            max === undefined
                ? undefinedBound
                : `${inclusiveMax ? "<=" : "<"} ${intl.formatNumber(max)}`;

        return `${minPart}${intl.formatMessage(RULE_MESSAGES.rangeSeparator)}${maxPart}`;
    };

    const formatRuleNode = (
        ruleType: string,
        params: Record<string, unknown>,
    ) => {
        switch (ruleType) {
            case "common.requiredNonEmpty":
                return intl.formatMessage(
                    RULE_MESSAGES["common.requiredNonEmpty"],
                );
            case "number.decimalScaleMax":
                return intl.formatMessage(
                    RULE_MESSAGES["number.decimalScaleMax"],
                    {
                        max: toNumber(params.max) ?? 0,
                    },
                );
            case "number.integerOnly":
                return intl.formatMessage(RULE_MESSAGES["number.integerOnly"]);
            case "number.notInSet":
                return intl.formatMessage(RULE_MESSAGES["number.notInSet"], {
                    values: toStringArray(params.values).join(", "),
                });
            case "number.range":
                return intl.formatMessage(RULE_MESSAGES["number.range"], {
                    range: formatRange(params),
                });
            case "text.allowedValues":
                return intl.formatMessage(RULE_MESSAGES["text.allowedValues"], {
                    values: toStringArray(params.values).join(", "),
                });
            case "text.maxLength":
                return intl.formatMessage(RULE_MESSAGES["text.maxLength"], {
                    max: toNumber(params.max) ?? 0,
                });
            case "text.minLength":
                return intl.formatMessage(RULE_MESSAGES["text.minLength"], {
                    min: toNumber(params.min) ?? 0,
                });
            case "text.notRegex":
                return intl.formatMessage(RULE_MESSAGES["text.notRegex"], {
                    pattern: toString(params.pattern) ?? "",
                });
            case "text.prefix":
                return intl.formatMessage(RULE_MESSAGES["text.prefix"], {
                    prefix: toString(params.prefix) ?? "",
                });
            case "text.regex":
                return intl.formatMessage(RULE_MESSAGES["text.regex"], {
                    pattern: toString(params.pattern) ?? "",
                });
            case "text.suffix":
                return intl.formatMessage(RULE_MESSAGES["text.suffix"], {
                    suffix: toString(params.suffix) ?? "",
                });
            default:
                return intl.formatMessage(RULE_MESSAGES.unknownRule, {
                    ruleType,
                });
        }
    };

    return {
        formatValidationRulePath: (
            path: string,
            validationCondition: RoiValidationConditionType,
        ): string => {
            const node = resolveNodeAtPath(path, validationCondition);
            if (!node) {
                return intl.formatMessage(RULE_MESSAGES.unknownPath, { path });
            }

            if (node.type === "group") {
                return intl.formatMessage(RULE_MESSAGES.group, {
                    operator: node.operator,
                });
            }

            return formatRuleNode(node.ruleType, node.params ?? {});
        },
        resolveNodeAtPath,
    };
};
