import type {
    RoiValidationConditionNodeType,
    RoiValidationRuleCatalogType,
} from "@/modules/rois/validation/schema";

export type RoiValidationRuleDefinition =
    RoiValidationRuleCatalogType["roiTypes"][number]["rules"][number];

export type NodeUpdater = (
    path: number[],
    updater: (
        node: RoiValidationConditionNodeType,
    ) => RoiValidationConditionNodeType,
) => void;
