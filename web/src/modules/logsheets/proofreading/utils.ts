import type { ExtractedValueType } from "@/modules/logsheets/schema";

export const getExtractedValueDefaultFormValue = (
    extractedValue: Pick<
        ExtractedValueType,
        "roiType" | "value" | "correctedValue"
    >,
): string | number | null => {
    const raw = extractedValue.correctedValue ?? extractedValue.value;

    if (!raw) {
        return null;
    }

    switch (extractedValue.roiType) {
        case "Number": {
            const n = parseFloat(raw);
            return isNaN(n) ? null : n;
        }
        case "Checkbox":
            return raw === "True" || raw === "true" ? "True" : "False";
        default:
            return raw;
    }
};
