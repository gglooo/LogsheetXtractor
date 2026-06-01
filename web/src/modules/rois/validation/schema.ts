import { roiTypeSchema } from "@/modules/rois/roi-type-schema";
import z from "zod";

export type RoiValidationConditionRuleType = {
    type: "rule";
    ruleType: string;
    params: Record<string, unknown>;
    schemaVersion?: number | null;
};

export type RoiValidationConditionGroupType = {
    type: "group";
    operator: "AND" | "OR";
    children: RoiValidationConditionNodeType[];
    schemaVersion?: number | null;
};

export type RoiValidationConditionNodeType =
    | RoiValidationConditionGroupType
    | RoiValidationConditionRuleType;

const jsonPrimitiveSchema = z.union([
    z.string(),
    z.number(),
    z.boolean(),
    z.null(),
]);

export const jsonValueSchema: z.ZodType<unknown> = z.lazy(() =>
    z.union([
        jsonPrimitiveSchema,
        z.array(jsonValueSchema),
        z.record(z.string(), jsonValueSchema),
    ]),
);

export const roiValidationConditionRuleSchema = z.object({
    type: z.literal("rule"),
    ruleType: z.string().min(1),
    params: z
        .record(z.string(), z.unknown())
        .nullish()
        .transform((value) => value ?? {}),
    schemaVersion: z.number().int().nullable().optional(),
});

export const roiValidationConditionNodeSchema: z.ZodType<RoiValidationConditionNodeType> =
    z.lazy(() =>
        z.union([
            roiValidationConditionGroupSchema,
            roiValidationConditionRuleSchema,
        ]),
    );

export const roiValidationConditionGroupSchema: z.ZodType<RoiValidationConditionGroupType> =
    z.lazy(() =>
        z.object({
            type: z.literal("group"),
            operator: z.enum(["AND", "OR"]),
            children: z.array(roiValidationConditionNodeSchema).min(1),
            schemaVersion: z.number().int().nullable().optional(),
        }),
    );

export const roiValidationConditionSchema = roiValidationConditionGroupSchema
    .nullish()
    .transform((value) => value ?? null);

export type RoiValidationConditionType = z.infer<
    typeof roiValidationConditionSchema
>;

export const roiValidationRuleDefinitionSchema = z.object({
    ruleType: z.string(),
    label: z.string(),
    description: z.string(),
    defaultParams: z
        .record(z.string(), jsonValueSchema)
        .nullish()
        .transform((value) => value ?? {}),
});

export const roiValidationRulesByRoiTypeSchema = z.object({
    roiType: roiTypeSchema,
    rules: z.array(roiValidationRuleDefinitionSchema),
});

export const roiValidationRuleCatalogSchema = z.object({
    version: z.string(),
    roiTypes: z.array(roiValidationRulesByRoiTypeSchema),
});

export type RoiValidationRuleCatalogType = z.infer<
    typeof roiValidationRuleCatalogSchema
>;

export const predefinedRoiValidationConditionSchema = z.object({
    id: z.uuid(),
    code: z.string().min(1),
    label: z.string().min(1),
    roiType: roiTypeSchema,
    condition: roiValidationConditionGroupSchema,
});

export const predefinedRoiValidationConditionListSchema = z.array(
    predefinedRoiValidationConditionSchema,
);

export type PredefinedRoiValidationConditionType = z.infer<
    typeof predefinedRoiValidationConditionSchema
>;
