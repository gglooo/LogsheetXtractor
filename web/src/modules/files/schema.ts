import { baseSchema } from "@/schema";
import z from "zod";

export const fileSchema = baseSchema.extend({
    fileName: z.string(),
    contentType: z.string(),
    sizeBytes: z.number().min(0),
});

export type FileType = z.infer<typeof fileSchema>;
