import { z } from "zod";

export const datedObjectSchema = z.object({
    createdAt: z.string().refine((date) => !isNaN(Date.parse(date)), {
        message: "Invalid date format",
    }),
    updatedAt: z
        .string()
        .refine((date) => !isNaN(Date.parse(date)), {
            message: "Invalid date format",
        })
        .nullish(),
    deletedAt: z
        .string()
        .refine((date) => !isNaN(Date.parse(date)), {
            message: "Invalid date format",
        })
        .nullish(),
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
