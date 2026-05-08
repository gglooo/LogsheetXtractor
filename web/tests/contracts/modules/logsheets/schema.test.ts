import { describe, expect, it } from "vitest";
import {
    createExtractedValueFormSchema,
    extractedValueSchema,
    logsheetSchema,
    uploadLogsheetsRequestSchema,
} from "@/modules/logsheets/schema";

const now = new Date().toISOString();
const id = {
    template: "11111111-1111-4111-8111-111111111111",
    file: "22222222-2222-4222-8222-222222222222",
    logsheet: "33333333-3333-4333-8333-333333333333",
    roi: "44444444-4444-4444-8444-444444444444",
    extracted: "55555555-5555-4555-8555-555555555555",
};

describe("logsheets schema contracts", () => {
    it("applies default validationWarnings for extracted values", () => {
        const parsed = extractedValueSchema.parse({
            id: id.extracted,
            createdAt: now,
            updatedAt: null,
            deletedAt: null,
            logsheetId: id.logsheet,
            roiId: id.roi,
            roiType: "Handwritten",
            variableName: "studentName",
            value: "Alice",
            correctedValue: null,
            status: "Unverified",
        });

        expect(parsed.validationWarnings).toEqual([]);
    });

    it("coerces number form value and validates checkbox form values", () => {
        const numberForm = createExtractedValueFormSchema("Number");
        expect(numberForm.parse({ correctedValue: "12.3" })).toEqual({
            correctedValue: 12.3,
        });

        const checkboxForm = createExtractedValueFormSchema("Checkbox");
        expect(checkboxForm.parse({ correctedValue: "True" })).toEqual({
            correctedValue: "True",
        });
        expect(() =>
            checkboxForm.parse({ correctedValue: "invalid" }),
        ).toThrow();
    });

    it("validates upload payload ids", () => {
        expect(
            uploadLogsheetsRequestSchema.safeParse({
                templateId: id.template,
                fileIds: [id.file],
                performAutomaticAlignment: true,
            }).success,
        ).toBe(true);

        expect(
            uploadLogsheetsRequestSchema.safeParse({
                templateId: "not-a-guid",
                fileIds: [id.file],
                performAutomaticAlignment: true,
            }).success,
        ).toBe(false);
    });

    it("parses full logsheet payload", () => {
        const parsed = logsheetSchema.parse({
            id: id.logsheet,
            createdAt: now,
            updatedAt: null,
            deletedAt: null,
            template: {
                id: id.template,
                createdAt: now,
                updatedAt: null,
                deletedAt: null,
                name: "Template A",
                parentId: null,
                backsideTemplateId: null,
                fileId: id.file,
                roiCount: 0,
                logsheetCount: 0,
                width: 1000,
                height: 1400,
            },
            file: {
                id: id.file,
                createdAt: now,
                updatedAt: null,
                deletedAt: null,
                fileName: "logsheet.pdf",
                contentType: "application/pdf",
                sizeBytes: 10,
            },
            status: "Pending",
            processedAt: null,
            alignmentData: null,
            extractedValues: [],
        });

        expect(parsed.status).toBe("Pending");
    });
});
