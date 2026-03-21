import { useLogsheet } from "@/modules/logsheets/api";
import { getValidationConditionByRoiId } from "@/modules/logsheets/proofreading/utils/validation-conditions";
import type { RoiValidationConditionType } from "@/modules/rois/validation/schema";
import { useTemplate } from "@/modules/templates/api";
import { useMemo } from "react";

export const useExtractedValueValidationCondition = (
    logsheetId: string,
    roiId: string,
): RoiValidationConditionType => {
    const { data: logsheet } = useLogsheet(logsheetId);
    const templateId = logsheet?.template.id ?? "";
    const { data: template } = useTemplate(templateId, Boolean(templateId));

    const backsideTemplateId = template?.backsideTemplate?.id ?? "";
    const { data: backsideTemplate } = useTemplate(
        backsideTemplateId,
        Boolean(backsideTemplateId),
    );

    return useMemo(
        () => getValidationConditionByRoiId(roiId, template, backsideTemplate),
        [roiId, template, backsideTemplate],
    );
};
