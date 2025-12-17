import { baseSchema, coordinateSchema } from "@/schema";
import z from "zod";

export const roiTypeSchema = z.enum([
    "Handwritten",
    "Number",
    "Checkbox",
    "Barcode",
]);

export const roiSchema = baseSchema.extend({
    variableName: z.string(),
    templateId: z.uuid(),
    type: roiTypeSchema,
    coordinates: coordinateSchema,
});

export type Roi = z.infer<typeof roiTypeSchema>;
