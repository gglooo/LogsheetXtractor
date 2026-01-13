import { fileSchema } from "@/modules/files/schema";
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
    "Failed",
    "NeedsReview",
    "Completed",
]);

export type LogsheetStatus = z.infer<typeof logsheetStatusSchema>;

export const verificationStatusSchema = z.enum(["Unverified", "Verified"]);

export type VerificationStatus = z.infer<typeof verificationStatusSchema>;

export const extractedValueSchema = baseSchema.extend({
    logsheetId: z.guid(),
    roiId: z.guid(),
    variableName: z.string(),
    value: z.string(),
    correctedValue: z.string().nullable(),
    status: verificationStatusSchema,
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
    backsideTemplate: templateListSchema.nullable(),
    file: fileSchema,
    status: logsheetStatusSchema,
    processedAt: dateSchema.nullable(),
    alignmentData: logsheetAlignmentDataContainerSchema.nullable(),
    extractedValues: z.array(extractedValueSchema),
});

export type LogsheetType = z.infer<typeof logsheetSchema>;

export const logsheetListSchema = baseSchema.extend({
    templateId: z.guid(),
    backsideTemplateId: z.guid().nullable(),
    file: fileSchema,
    status: logsheetStatusSchema,
    processedAt: dateSchema.nullable(),
});

export type LogsheetListType = z.infer<typeof logsheetListSchema>;
