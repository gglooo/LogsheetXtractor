import { roiTypeSchema } from "@/modules/rois/schema";
import z from "zod";

export const editRoiSchema = z.object({
    variableName: z.string().min(1),
    type: roiTypeSchema,
});

export type EditRoiFormValues = z.infer<typeof editRoiSchema>;
