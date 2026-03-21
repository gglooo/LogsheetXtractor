import { describe, expect, it } from "vitest";
import { LogsheetStateMachine } from "@/modules/logsheets/state-machine";

describe("LogsheetStateMachine", () => {
    it("allows proofreading only for NeedsReview", () => {
        expect(
            LogsheetStateMachine.fromStatus("NeedsReview").canProofread(),
        ).toBe(true);
        expect(LogsheetStateMachine.fromStatus("Pending").canProofread()).toBe(
            false,
        );
    });

    it("allows align and process for Pending and Failed", () => {
        expect(LogsheetStateMachine.fromStatus("Pending").canAlign()).toBe(
            true,
        );
        expect(LogsheetStateMachine.fromStatus("Failed").canAlign()).toBe(
            true,
        );
        expect(
            LogsheetStateMachine.fromStatus("Processing").canAlign(),
        ).toBe(false);

        expect(LogsheetStateMachine.fromStatus("Pending").canProcess()).toBe(
            true,
        );
        expect(LogsheetStateMachine.fromStatus("Failed").canProcess()).toBe(
            true,
        );
        expect(
            LogsheetStateMachine.fromStatus("Completed").canProcess(),
        ).toBe(false);
    });

    it("blocks delete during processing and aligning", () => {
        expect(
            LogsheetStateMachine.fromStatus("Processing").canDelete(),
        ).toBe(false);
        expect(LogsheetStateMachine.fromStatus("Aligning").canDelete()).toBe(
            false,
        );
        expect(LogsheetStateMachine.fromStatus("Failed").canDelete()).toBe(
            true,
        );
    });
});
