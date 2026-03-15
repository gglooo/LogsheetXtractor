import { fileSchema } from "@/modules/files/schema";
import { residualSchema } from "@/modules/residuals/schema";
import { roiSchema } from "@/modules/rois/schema";
import { baseSchema, pdfFileSchema } from "@/schema";
import z from "zod";

export const templateListSchema = baseSchema.extend({
    name: z.string(),
    parentId: z.uuid().nullable(),
    backsideTemplateId: z.uuid().nullable(),
    fileId: z.uuid().nullable(),
    roiCount: z.number().min(0),
    logsheetCount: z.number().min(0),
    width: z.number(),
    height: z.number(),
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

const baseTemplateDetailSchema = baseSchema.extend({
    name: z.string(),
    parent: templateWithoutParentSchema.nullable(),
    width: z.number(),
    height: z.number(),
    file: fileSchema.nullable(),
    rois: z.array(roiSchema),
    residuals: z.array(residualSchema),
    isEditable: z.boolean(),
});

const templateReferenceSchema = z.object({
    id: z.guid(),
    name: z.string(),
    width: z.number(),
    height: z.number(),
    fileId: z.guid(),
});

export const templateSchema = baseTemplateDetailSchema.extend({
    frontsideTemplate: templateReferenceSchema.nullish(),
    backsideTemplate: templateReferenceSchema.nullish(),
});

export const baseCreateTemplateSchema = z.object({
    name: z.string().min(1).trim(),
    file: pdfFileSchema,
    importedConfig: z.instanceof(File).optional(),
});

export const createTemplateSchema = baseCreateTemplateSchema
    .extend({
        backside: baseCreateTemplateSchema.optional(),
    })
    .refine((data) => data.name !== data.backside?.name, {
        message: "The template name and backside name cannot be the same.",
        path: ["backside", "name"],
    })
    .refine((data) => data.name !== data.backside?.name, {
        message: "The template file and backside file cannot be the same.",
        path: ["name"],
    });

export type CreateTemplateFormValues = z.infer<typeof createTemplateSchema>;

export type TemplateType = z.infer<typeof templateSchema>;

export const baseCloneTemplateSchema = z.object({
    name: z.string().min(1).trim(),
    file: pdfFileSchema,
});

export const cloneTemplateSchema = baseCloneTemplateSchema
    .extend({
        backside: baseCloneTemplateSchema.optional(),
    })
    .refine((data) => data.name !== data.backside?.name, {
        message: "The template name and backside name cannot be the same.",
        path: ["backside", "name"],
    })
    .refine((data) => data.name !== data.backside?.name, {
        message: "The template file and backside file cannot be the same.",
        path: ["name"],
    });

export type CloneTemplateFormValues = z.infer<typeof cloneTemplateSchema>;

export const addTemplateBacksideSchema = z.object({
    backside: z.object({
        file: pdfFileSchema,
        name: z.string().min(1).trim(),
        importedConfig: z.instanceof(File).optional(),
    }),
});

export type AddTemplateBacksideFormValues = z.infer<
    typeof addTemplateBacksideSchema
>;
