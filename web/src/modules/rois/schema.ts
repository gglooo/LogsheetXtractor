import type { SelectOption } from "@/components/form/form-select";
import { detectedResidualSchema } from "@/modules/residuals/schema";
import { roiTypeSchema } from "@/modules/rois/roi-type-schema";
import { roiValidationConditionSchema } from "@/modules/rois/validation/schema";
import { baseSchema, coordinateSchema } from "@/schema";
import z from "zod";

export const roiTypeSelectOptions: SelectOption[] = roiTypeSchema.options.map(
    (type) => ({
        label: type,
        value: type,
    }),
);

export const roiSchema = baseSchema.extend({
    id: z.uuid(),
    variableName: z.string(),
    templateId: z.uuid(),
    type: roiTypeSchema,
    coordinates: coordinateSchema,
    validationCondition: roiValidationConditionSchema,
});

export type RoiType = z.infer<typeof roiSchema>;

export const detectedRoiSchema = roiSchema.extend({
    id: z.uuid().nullable(),
    type: roiTypeSchema.nullable(),
});

export type DetectedRoiType = z.infer<typeof detectedRoiSchema>;

export const setRoiSchema = z.object({
    id: z.uuid().nullable(),
    variableName: z.string(),
    type: roiTypeSchema.nullable(),
    coordinates: coordinateSchema,
    validationCondition: roiValidationConditionSchema,
});

export type SetRoiType = z.infer<typeof setRoiSchema>;

export const setRoisRequestSchema = z.object({
    rois: z.array(setRoiSchema),
});

export const detectRoisResponseSchema = z.object({
    rois: z.array(detectedRoiSchema),
    residuals: z.array(detectedResidualSchema),
});

export type DetectRoiResponseType = z.infer<typeof detectRoisResponseSchema>;
