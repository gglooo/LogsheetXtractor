import z from "zod";

export const roiTypeSchema = z.enum([
    "Handwritten",
    "Number",
    "Checkbox",
    "Barcode",
]);

export type RoiTypeEnum = z.infer<typeof roiTypeSchema>;
