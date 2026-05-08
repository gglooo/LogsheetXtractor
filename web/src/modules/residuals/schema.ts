import { baseSchema, coordinateSchema } from "@/schema";
import z from "zod";

export const residualSchema = baseSchema.extend({
    templateId: z.uuid(),
    content: z.string(),
    coordinates: coordinateSchema,
});

export type ResidualType = z.infer<typeof residualSchema>;

export const detectedResidualSchema = residualSchema.extend({
    id: z.uuid().nullable(),
});

export type DetectedResidualType = z.infer<typeof detectedResidualSchema>;
