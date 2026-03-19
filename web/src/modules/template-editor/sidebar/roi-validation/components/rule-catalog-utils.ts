import type { RoiType } from "@/modules/rois/schema";
import type {
    RoiValidationConditionGroupType,
    RoiValidationConditionRuleType,
    RoiValidationRuleCatalogType,
} from "@/modules/rois/validation/schema";

const cloneJson = <T>(value: T): T => JSON.parse(JSON.stringify(value)) as T;

export const toRecord = (value: unknown): Record<string, unknown> => {
    if (typeof value === "object" && value !== null && !Array.isArray(value)) {
        return cloneJson(value as Record<string, unknown>);
    }
    return {};
};

export const getRulesForRoiType = (
    catalog: RoiValidationRuleCatalogType,
    roiType: RoiType["type"],
) => {
    return (
        catalog.roiTypes.find((item) => item.roiType === roiType)?.rules ?? []
    );
};

export const buildDefaultRuleNode = (
    catalog: RoiValidationRuleCatalogType,
    roiType: RoiType["type"],
): RoiValidationConditionRuleType | null => {
    const firstRule = getRulesForRoiType(catalog, roiType)[0];
    if (!firstRule) {
        return null;
    }

    return {
        type: "rule",
        ruleType: firstRule.ruleType,
        params: toRecord(firstRule.defaultParams),
    };
};

export const buildDefaultGroupNode = (
    catalog: RoiValidationRuleCatalogType,
    roiType: RoiType["type"],
): RoiValidationConditionGroupType | null => {
    const firstRule = buildDefaultRuleNode(catalog, roiType);
    if (!firstRule) {
        return null;
    }

    return {
        type: "group",
        operator: "AND",
        children: [firstRule],
    };
};
