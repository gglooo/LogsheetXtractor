import { roiTypeSchema, type RoiTypeEnum } from "@/modules/rois/roi-type-schema";
import {
    predefinedRoiValidationConditionListSchema,
    roiValidationRuleCatalogSchema,
} from "@/modules/rois/validation/schema";
import { useQuery } from "@tanstack/react-query";

export const useRoiValidationRuleCatalog = () =>
    useQuery({
        queryKey: ["roi-validation-rules"],
        queryFn: async () => {
            const response = await fetch("/api/roi-validation/rules");
            return await roiValidationRuleCatalogSchema.parseAsync(
                await response.json(),
            );
        },
    });

export const usePredefinedRoiValidationConditions = (roiType?: RoiTypeEnum) =>
    useQuery({
        queryKey: ["roi-validation-predefined-conditions", roiType ?? null],
        queryFn: async () => {
            const parsedRoiType = roiType
                ? roiTypeSchema.parse(roiType)
                : undefined;

            const url = parsedRoiType
                ? `/api/roi-validation/predefined-conditions?roiType=${encodeURIComponent(parsedRoiType)}`
                : "/api/roi-validation/predefined-conditions";
            const response = await fetch(url);
            return await predefinedRoiValidationConditionListSchema.parseAsync(
                await response.json(),
            );
        },
    });
