import { baseSchema, coordinateSchema } from "@/schema";
import z from "zod";

export const residualSchema = baseSchema.extend({
    templateId: z.uuid(),
    content: z.string(),
    coordinates: coordinateSchema,
});

export type Residual = z.infer<typeof residualSchema>;
