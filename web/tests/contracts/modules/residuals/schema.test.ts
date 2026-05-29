import {
    detectedResidualSchema,
    residualSchema,
} from "@/modules/residuals/schema";
import { describe, expect, it } from "vitest";

const now = new Date().toISOString();

const validResidual = {
    id: "11111111-1111-4111-8111-111111111111",
    createdAt: now,
    updatedAt: null,
    deletedAt: null,
    templateId: "22222222-2222-4222-8222-222222222222",
    content: "dust",
    coordinates: {
        x: 1,
        y: 2,
        width: 3,
        height: 4,
    },
};

describe("residual schema contracts", () => {
    it("parses backend residual payload", () => {
        expect(residualSchema.parse(validResidual).content).toBe("dust");
    });

    it("rejects renamed coordinate fields", () => {
        const result = residualSchema.safeParse({
            ...validResidual,
            coordinates: {
                left: 1,
                top: 2,
                width: 3,
                height: 4,
            },
        });

        expect(result.success).toBe(false);
    });

    it("allows detected residuals without persisted id", () => {
        const result = detectedResidualSchema.parse({
            ...validResidual,
            id: null,
        });

        expect(result.id).toBeNull();
    });
});
