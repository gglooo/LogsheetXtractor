import { fileSchema } from "@/modules/files/schema";
import { residualSchema } from "@/modules/residuals/schema";
import { roiSchema } from "@/modules/rois/schema";
import { baseSchema } from "@/schema";
import z from "zod";

export const templateListSchema = baseSchema.extend({
    name: z.string(),
    parentId: z.uuid().nullable(),
    fileId: z.uuid().nullable(),
    roiCount: z.number().min(0),
});

export type TemplateListItemType = z.infer<typeof templateListSchema>;

export const templateWithoutParentSchema = baseSchema.extend({
    name: z.string(),
    width: z.number(),
    height: z.number(),
    file: fileSchema.nullable(),
});

export type TemplateWithoutParentType = z.infer<
    typeof templateWithoutParentSchema
>;

export const templateSchema = baseSchema.extend({
    name: z.string(),
    parent: templateWithoutParentSchema.nullable(),
    width: z.number(),
    height: z.number(),
    file: fileSchema.nullable(),
    rois: z.array(roiSchema),
    residuals: z.array(residualSchema),
});
