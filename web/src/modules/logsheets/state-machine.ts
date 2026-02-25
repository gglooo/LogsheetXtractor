import type { LogsheetStatus } from "./schema";

export class LogsheetStateMachine {
    private readonly status: LogsheetStatus;

    constructor(status: LogsheetStatus) {
        this.status = status;
    }

    static fromStatus(status: LogsheetStatus) {
        return new LogsheetStateMachine(status);
    }

    canProofread(): boolean {
        return this.status === "NeedsReview";
    }

    canAlign(): boolean {
        return this.status === "Pending" || this.status === "Failed";
    }

    canProcess(): boolean {
        return this.status === "Pending" || this.status === "Failed";
    }

    canDelete(): boolean {
        return this.status !== "Processing";
    }
}
