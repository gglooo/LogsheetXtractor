import { z } from "zod";

export const dateSchema = z
    .string()
    .refine((date) => !isNaN(Date.parse(date)), {
        message: "Invalid date format",
    });

export const datedObjectSchema = z.object({
    createdAt: dateSchema,
    updatedAt: dateSchema.nullish(),
    deletedAt: dateSchema.nullish(),
});

export type DatedObject = z.infer<typeof datedObjectSchema>;

export const idSchema = z.object({
    id: z.uuid(),
});

export type Id = z.infer<typeof idSchema>;

export const baseSchema = idSchema.extend(datedObjectSchema.shape);

export const coordinateSchema = z.object({
    x: z.number(),
    y: z.number(),
    width: z.number(),
    height: z.number(),
});

export type Coordinates = z.infer<typeof coordinateSchema>;

export const pointCoordinateSchema = z.object({
    x: z.number(),
    y: z.number(),
});

export type PointCoordinates = z.infer<typeof pointCoordinateSchema>;

export const dimensionsSchema = z.object({
    width: z.number(),
    height: z.number(),
});

export type Dimensions = z.infer<typeof dimensionsSchema>;

export const pdfFileSchema = z
    .file()
    .refine((file) => file.type === "application/pdf");
