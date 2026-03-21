import type { RoiValidationConditionType } from "@/modules/rois/validation/schema";
import type { TemplateType } from "@/modules/templates/schema";

export const getValidationConditionsByRoiId = (
    template?: TemplateType,
    backsideTemplate?: TemplateType,
): Record<string, RoiValidationConditionType> => {
    const map: Record<string, RoiValidationConditionType> = {};

    (template?.rois ?? []).forEach((roi) => {
        map[roi.id] = roi.validationCondition;
    });

    (backsideTemplate?.rois ?? []).forEach((roi) => {
        map[roi.id] = roi.validationCondition;
    });

    return map;
};

export const getValidationConditionByRoiId = (
    roiId: string,
    template?: TemplateType,
    backsideTemplate?: TemplateType,
): RoiValidationConditionType =>
    getValidationConditionsByRoiId(template, backsideTemplate)[roiId] ?? null;
