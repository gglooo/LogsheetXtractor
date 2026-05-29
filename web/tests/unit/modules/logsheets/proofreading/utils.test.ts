import { describe, expect, it } from "vitest";
import { getExtractedValueDefaultFormValue } from "@/modules/logsheets/proofreading/utils";

describe("proofreading utils", () => {
    it("returns null when no raw value exists", () => {
        const value = getExtractedValueDefaultFormValue({
            roiType: "Handwritten",
            value: "",
            correctedValue: null,
        });

        expect(value).toBeNull();
    });

    it("prefers corrected value over original", () => {
        const value = getExtractedValueDefaultFormValue({
            roiType: "Handwritten",
            value: "raw",
            correctedValue: "corrected",
        });

        expect(value).toBe("corrected");
    });

    it("coerces number strings to numbers", () => {
        expect(
            getExtractedValueDefaultFormValue({
                roiType: "Number",
                value: "12.5",
                correctedValue: null,
            }),
        ).toBe(12.5);

        expect(
            getExtractedValueDefaultFormValue({
                roiType: "Number",
                value: "not-a-number",
                correctedValue: null,
            }),
        ).toBeNull();
    });

    it("normalizes checkbox values to True/False", () => {
        expect(
            getExtractedValueDefaultFormValue({
                roiType: "Checkbox",
                value: "true",
                correctedValue: null,
            }),
        ).toBe("True");

        expect(
            getExtractedValueDefaultFormValue({
                roiType: "Checkbox",
                value: "anything-else",
                correctedValue: null,
            }),
        ).toBe("False");
    });
});
