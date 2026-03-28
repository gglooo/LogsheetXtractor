import { describe, expect, it } from "vitest";
import {
    addTemplateBacksideSchema,
    cloneTemplateSchema,
    createTemplateSchema,
    templateSchema,
} from "@/modules/templates/schema";

const now = new Date().toISOString();

const pdfFile = () =>
    new File(["%PDF-sample"], "sample.pdf", { type: "application/pdf" });

const textFile = () =>
    new File(["text"], "sample.txt", { type: "text/plain" });

describe("templates schema contracts", () => {
    it("parses create template payload with optional backside", () => {
        const parsed = createTemplateSchema.parse({
            name: "Template A",
            file: pdfFile(),
            backside: {
                file: pdfFile(),
            },
        });

        expect(parsed.name).toBe("Template A");
        expect(parsed.backside?.file.type).toBe("application/pdf");
    });

    it("rejects non-pdf files for template creation", () => {
        expect(() =>
            createTemplateSchema.parse({
                name: "Template A",
                file: textFile(),
            }),
        ).toThrow();
    });

    it("parses clone payload with optional backside file", () => {
        const result = cloneTemplateSchema.safeParse({
            name: "Same",
            file: pdfFile(),
            backside: {
                file: pdfFile(),
            },
        });

        expect(result.success).toBe(true);
    });

    it("parses template details payload", () => {
        const parsed = templateSchema.parse({
            id: "11111111-1111-4111-8111-111111111111",
            createdAt: now,
            updatedAt: null,
            deletedAt: null,
            name: "Template A",
            parent: null,
            width: 1000,
            height: 1400,
            file: null,
            rois: [],
            residuals: [],
            isEditable: true,
            frontsideTemplate: null,
            backsideTemplate: null,
        });

        expect(parsed.isEditable).toBe(true);
    });

    it("requires backside pdf file for add backside schema", () => {
        expect(
            addTemplateBacksideSchema.safeParse({
                backside: {
                    file: pdfFile(),
                },
            }).success,
        ).toBe(true);

        expect(
            addTemplateBacksideSchema.safeParse({
                backside: {
                    file: textFile(),
                },
            }).success,
        ).toBe(false);
    });
});
