import { fileSchema } from "@/modules/files/schema";
import { roiTypeSchema } from "@/modules/rois/roi-type-schema";
import { templateListSchema } from "@/modules/templates/schema";
import {
    baseSchema,
    dateSchema,
    dimensionsSchema,
    pointCoordinateSchema,
} from "@/schema";
import z from "zod";

export const logsheetStatusSchema = z.enum([
    "Pending",
    "Aligning",
    "Failed",
    "NeedsReview",
    "Completed",
    "Processing",
]);

export type LogsheetStatus = z.infer<typeof logsheetStatusSchema>;

export const verificationStatusSchema = z.enum(["Unverified", "Verified"]);

export type VerificationStatus = z.infer<typeof verificationStatusSchema>;

export const roiValidationWarningSchema = z.object({
    code: z.string(),
    message: z.string(),
    path: z.string(),
});

export type RoiValidationWarningType = z.infer<
    typeof roiValidationWarningSchema
>;

export const extractedValueSchema = baseSchema.extend({
    logsheetId: z.guid(),
    roiId: z.guid(),
    roiType: roiTypeSchema,
    variableName: z.string(),
    value: z.string(),
    correctedValue: z.string().nullable(),
    status: verificationStatusSchema,
    validationWarnings: z.array(roiValidationWarningSchema).default([]),
    validationRulesVersion: z.string().nullable().optional(),
});

export type ExtractedValueType = z.infer<typeof extractedValueSchema>;

export const logsheetAlignmentDataSchema = z.object({
    dimensions: dimensionsSchema,
    templatePoints: z.array(pointCoordinateSchema),
    logsheetPoints: z.array(pointCoordinateSchema),
});

export type LogsheetAlignmentData = z.infer<typeof logsheetAlignmentDataSchema>;

export const logsheetAlignmentDataContainerSchema = z.object({
    frontside: z.any(),
    backside: z.any(),
});

export const logsheetSchema = baseSchema.extend({
    template: templateListSchema,
    file: fileSchema,
    status: logsheetStatusSchema,
    processedAt: dateSchema.nullable(),
    alignmentData: logsheetAlignmentDataContainerSchema.nullable(),
    extractedValues: z.array(extractedValueSchema),
});

export type LogsheetType = z.infer<typeof logsheetSchema>;

export const logsheetListSchema = baseSchema.extend({
    templateId: z.guid(),
    file: fileSchema,
    status: logsheetStatusSchema,
    processedAt: dateSchema.nullable(),
    isFrontAligned: z.boolean(),
    isBackAligned: z.boolean(),
    errorMessage: z.string().nullable(),
});

export type LogsheetListType = z.infer<typeof logsheetListSchema>;

export const createExtractedValueFormSchema = (
    roiType: z.infer<typeof roiTypeSchema>,
) => {
    switch (roiType) {
        case "Number":
            return z.object({
                correctedValue: z.coerce.number().nullable(),
            });
        case "Checkbox":
            return z.object({
                correctedValue: z.enum(["True", "False"]).nullable(),
            });
        case "Handwritten":
        case "Barcode":
        default:
            return z.object({
                correctedValue: z.string().nullable(),
            });
    }
};

export type ExtractedValueFormValues = z.infer<
    ReturnType<typeof createExtractedValueFormSchema>
>;

export const uploadLogsheetsRequestSchema = z.object({
    templateId: z.guid(),
    backsideTemplateId: z.guid().optional(),
    fileIds: z.array(z.guid()),
    performAutomaticAlignment: z.boolean(),
});

export type UploadLogsheetsRequest = z.infer<
    typeof uploadLogsheetsRequestSchema
>;
