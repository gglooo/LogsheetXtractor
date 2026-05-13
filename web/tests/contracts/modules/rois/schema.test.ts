import { describe, expect, it } from "vitest";
import {
    detectRoisResponseSchema,
    roiSchema,
    setRoisRequestSchema,
} from "@/modules/rois/schema";

const now = new Date().toISOString();

const baseCoordinates = {
    x: 10,
    y: 20,
    width: 30,
    height: 40,
};

describe("rois schema contracts", () => {
    it("parses ROI payload with nullable validation condition", () => {
        const parsed = roiSchema.parse({
            id: "11111111-1111-4111-8111-111111111111",
            createdAt: now,
            updatedAt: null,
            deletedAt: null,
            variableName: "studentName",
            templateId: "22222222-2222-4222-8222-222222222222",
            type: "Handwritten",
            coordinates: baseCoordinates,
            validationCondition: null,
        });

        expect(parsed.validationCondition).toBeNull();
    });

    it("parses set rois request with new and existing rois", () => {
        const parsed = setRoisRequestSchema.parse({
            rois: [
                {
                    id: null,
                    variableName: "newRoi",
                    type: null,
                    coordinates: baseCoordinates,
                    validationCondition: null,
                },
                {
                    id: "33333333-3333-4333-8333-333333333333",
                    variableName: "existingRoi",
                    type: "Number",
                    coordinates: baseCoordinates,
                    validationCondition: {
                        type: "group",
                        operator: "AND",
                        children: [
                            {
                                type: "rule",
                                ruleType: "number.integerOnly",
                                params: {},
                            },
                        ],
                    },
                },
            ],
        });

        expect(parsed.rois).toHaveLength(2);
    });

    it("rejects ROI response payloads with renamed variable field", () => {
        const result = roiSchema.safeParse({
            id: "11111111-1111-4111-8111-111111111111",
            createdAt: now,
            updatedAt: null,
            deletedAt: null,
            variable: "studentName",
            templateId: "22222222-2222-4222-8222-222222222222",
            type: "Handwritten",
            coordinates: baseCoordinates,
            validationCondition: null,
        });

        expect(result.success).toBe(false);
    });

    it("parses detect rois response with residuals", () => {
        const parsed = detectRoisResponseSchema.parse({
            rois: [
                {
                    id: null,
                    createdAt: now,
                    updatedAt: null,
                    deletedAt: null,
                    variableName: "detected",
                    templateId: "44444444-4444-4444-8444-444444444444",
                    type: "Checkbox",
                    coordinates: baseCoordinates,
                    validationCondition: null,
                },
            ],
            residuals: [
                {
                    id: null,
                    createdAt: now,
                    updatedAt: null,
                    deletedAt: null,
                    templateId: "44444444-4444-4444-8444-444444444444",
                    content: "noise",
                    coordinates: baseCoordinates,
                },
            ],
        });

        expect(parsed.rois[0].id).toBeNull();
        expect(parsed.residuals[0].id).toBeNull();
    });
});
