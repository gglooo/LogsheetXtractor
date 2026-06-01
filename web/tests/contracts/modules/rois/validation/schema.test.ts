import { describe, expect, it } from "vitest";
import {
    predefinedRoiValidationConditionSchema,
    roiValidationConditionGroupSchema,
    roiValidationConditionRuleSchema,
    roiValidationConditionSchema,
    roiValidationRuleCatalogSchema,
} from "@/modules/rois/validation/schema";

describe("roi validation schema contracts", () => {
    it("normalizes undefined condition to null", () => {
        expect(roiValidationConditionSchema.parse(undefined)).toBeNull();
        expect(roiValidationConditionSchema.parse(null)).toBeNull();
    });

    it("normalizes rule params to empty object", () => {
        const parsed = roiValidationConditionRuleSchema.parse({
            type: "rule",
            ruleType: "text.maxLength",
            params: null,
        });

        expect(parsed.params).toEqual({});
    });

    it("requires non-empty group children", () => {
        expect(
            roiValidationConditionGroupSchema.safeParse({
                type: "group",
                operator: "AND",
                children: [],
            }).success,
        ).toBe(false);
    });

    it("parses rule catalog with default params", () => {
        const parsed = roiValidationRuleCatalogSchema.parse({
            version: "1",
            roiTypes: [
                {
                    roiType: "Number",
                    rules: [
                        {
                            ruleType: "number.range",
                            label: "Range",
                            description: "Must be in range",
                            defaultParams: {
                                min: 0,
                                max: 10,
                            },
                        },
                    ],
                },
            ],
        });

        expect(parsed.roiTypes[0].rules[0].defaultParams).toEqual({
            min: 0,
            max: 10,
        });
    });

    it("parses predefined condition payload", () => {
        const parsed = predefinedRoiValidationConditionSchema.parse({
            id: "11111111-1111-4111-8111-111111111111",
            code: "required",
            label: "Required",
            roiType: "Handwritten",
            condition: {
                type: "group",
                operator: "AND",
                children: [
                    {
                        type: "rule",
                        ruleType: "common.requiredNonEmpty",
                        params: {},
                    },
                ],
            },
        });

        expect(parsed.condition.children).toHaveLength(1);
    });

    it("rejects predefined condition payloads with missing stable fields", () => {
        const result = predefinedRoiValidationConditionSchema.safeParse({
            id: "11111111-1111-4111-8111-111111111111",
            code: "required",
            roiType: "Handwritten",
            condition: {
                type: "group",
                operator: "AND",
                children: [
                    {
                        type: "rule",
                        ruleType: "common.requiredNonEmpty",
                        params: {},
                    },
                ],
            },
        });

        expect(result.success).toBe(false);
    });

    it("parses nested validation groups and rule params", () => {
        const parsed = roiValidationConditionGroupSchema.parse({
            type: "group",
            operator: "OR",
            children: [
                {
                    type: "group",
                    operator: "AND",
                    children: [
                        {
                            type: "rule",
                            ruleType: "text.prefix",
                            params: { value: "LSI" },
                        },
                    ],
                },
                {
                    type: "rule",
                    ruleType: "text.allowedValues",
                    params: { values: ["N/A", "OK"] },
                },
            ],
        });

        expect(parsed.children).toHaveLength(2);
        expect(parsed.children[0].type).toBe("group");
    });
});
