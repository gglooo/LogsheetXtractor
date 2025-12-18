import { baseSchema } from "@/schema";
import z from "zod";

export const fileSchema = baseSchema.extend({
    fileName: z.string(),
    contentType: z.string(),
    sizeBytes: z.number().min(0),
});

export type FileType = z.infer<typeof fileSchema>;

export const downloadedFileSchema = z.object({
    bytes: z.instanceof(ArrayBuffer),
    fileName: z.string(),
    contentType: z.string().nullable(),
});

export type DownloadedFileType = z.infer<typeof downloadedFileSchema>;
