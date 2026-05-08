import { ExtractedValueForm } from "@/modules/logsheets/proofreading/components/extracted-value-form";
import type { ExtractedValueType } from "@/modules/logsheets/schema";
import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, describe, expect, it, vi } from "vitest";
import { renderWithProviders } from "../../../../utils/render-with-providers";

const originalFetch = window.fetch;

const ids = {
    logsheet: "33333333-3333-4333-8333-333333333333",
    roi: "44444444-4444-4444-8444-444444444444",
    extracted: "55555555-5555-4555-8555-555555555555",
};

const extractedValue: ExtractedValueType = {
    id: ids.extracted,
    createdAt: new Date().toISOString(),
    updatedAt: null,
    deletedAt: null,
    logsheetId: ids.logsheet,
    roiId: ids.roi,
    roiType: "Handwritten",
    variableName: "operator",
    value: "orignal",
    correctedValue: null,
    status: "Unverified",
    validationWarnings: [],
    validationRulesVersion: null,
};

afterEach(() => {
    window.fetch = originalFetch;
    vi.restoreAllMocks();
});

describe("ExtractedValueForm", () => {
    it("submits the corrected value when Enter is pressed in the selected value field", async () => {
        const user = userEvent.setup();
        const onVerified = vi.fn();
        const fetchMock = vi.fn().mockResolvedValue(
            new Response(JSON.stringify({ ok: true }), {
                status: 200,
                headers: { "Content-Type": "application/json" },
            }),
        );
        window.fetch = fetchMock as typeof fetch;

        renderWithProviders(
            <ExtractedValueForm
                extractedValue={extractedValue}
                validationCondition={null}
                onVerified={onVerified}
            />,
        );

        const correctedValueInput = screen.getByLabelText("Corrected value");
        await user.clear(correctedValueInput);
        await user.type(correctedValueInput, "original{Enter}");

        await waitFor(() => {
            expect(fetchMock).toHaveBeenCalledWith(
                `/api/extracted-values/${ids.extracted}/verify`,
                expect.objectContaining({
                    method: "POST",
                    body: JSON.stringify({ correctedValue: "original" }),
                }),
            );
        });
        expect(onVerified).toHaveBeenCalledWith(extractedValue);
    });
});
