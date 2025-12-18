import { detectedResidualSchema } from "@/modules/residuals/schema";
import { baseSchema, coordinateSchema } from "@/schema";
import z from "zod";

export const roiTypeSchema = z.enum([
    "Handwritten",
    "Number",
    "Checkbox",
    "Barcode",
]);

export const roiSchema = baseSchema.extend({
    id: z.uuid(),
    variableName: z.string(),
    templateId: z.uuid(),
    type: roiTypeSchema,
    coordinates: coordinateSchema,
});

export type RoiType = z.infer<typeof roiSchema>;

export const detectedRoiSchema = roiSchema.extend({
    id: z.uuid().nullable(),
    type: roiTypeSchema.nullable(),
});

export type DetectedRoiType = z.infer<typeof detectedRoiSchema>;

export const detectRoisResponseSchema = z.object({
    rois: z.array(detectedRoiSchema),
    residuals: z.array(detectedResidualSchema),
});

export type DetectRoiResponseType = z.infer<typeof detectRoisResponseSchema>;
