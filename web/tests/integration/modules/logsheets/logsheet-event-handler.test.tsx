import {
    LogsheetEventHandler,
    type LogsheetAutomaticAlignmentFinishedEvent,
    type LogsheetProcessingFinishedEvent,
} from "@/modules/logsheets/logsheet-event-handler";
import { SignalRContext } from "@/modules/signalr/signalr-context";
import { act, waitFor } from "@testing-library/react";
import { toast } from "sonner";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { renderWithProviders } from "../../../utils/render-with-providers";
import { createTestQueryClient } from "../../../utils/query-client";

type HandlerMap = {
    LogsheetProcessingFinished?: (
        event: LogsheetProcessingFinishedEvent,
    ) => Promise<void>;
    LogsheetAutomaticAlignmentFinished?: (
        event: LogsheetAutomaticAlignmentFinishedEvent,
    ) => Promise<void>;
};

const createConnection = (handlers: HandlerMap) => ({
    on: vi.fn((eventName: keyof HandlerMap, handler: HandlerMap[typeof eventName]) => {
        handlers[eventName] = handler;
    }),
    off: vi.fn((eventName: keyof HandlerMap, handler: HandlerMap[typeof eventName]) => {
        if (handlers[eventName] === handler) {
            delete handlers[eventName];
        }
    }),
});

vi.mock("sonner", () => ({
    toast: {
        success: vi.fn(),
        error: vi.fn(),
    },
}));

beforeEach(() => {
    localStorage.setItem("app-locale", "en");
});

afterEach(() => {
    vi.clearAllMocks();
});

describe("LogsheetEventHandler", () => {
    it("shows a success toast and invalidates logsheets after processing succeeds", async () => {
        const handlers: HandlerMap = {};
        const connection = createConnection(handlers);
        const queryClient = createTestQueryClient();
        const invalidateQueriesSpy = vi
            .spyOn(queryClient, "invalidateQueries")
            .mockResolvedValue();

        renderWithProviders(
            <SignalRContext.Provider
                value={{ connection: connection as never, isConnected: true }}
            >
                <LogsheetEventHandler />
            </SignalRContext.Provider>,
            { queryClient },
        );

        await act(async () => {
            await handlers.LogsheetProcessingFinished?.({
                logsheetId: "logsheet-1",
                isSuccess: true,
            });
        });

        expect(toast.success).toHaveBeenCalledWith(
            "Logsheet processing completed successfully",
            { id: "logsheet-processing-success" },
        );
        expect(invalidateQueriesSpy).toHaveBeenCalledWith({
            queryKey: ["logsheets"],
        });
    });

    it("shows an error toast with details after processing fails", async () => {
        const handlers: HandlerMap = {};
        const connection = createConnection(handlers);

        renderWithProviders(
            <SignalRContext.Provider
                value={{ connection: connection as never, isConnected: true }}
            >
                <LogsheetEventHandler />
            </SignalRContext.Provider>,
        );

        await act(async () => {
            await handlers.LogsheetProcessingFinished?.({
                logsheetId: "logsheet-2",
                isSuccess: false,
                errorMessage: "OCR timed out",
            });
        });

        expect(toast.error).toHaveBeenCalledWith("Logsheet processing failed", {
            id: "logsheet-processing-error-logsheet-2",
            description: "OCR timed out",
        });
    });

    it("invalidates logsheets and reports automatic alignment failures", async () => {
        const handlers: HandlerMap = {};
        const connection = createConnection(handlers);
        const queryClient = createTestQueryClient();
        const invalidateQueriesSpy = vi
            .spyOn(queryClient, "invalidateQueries")
            .mockResolvedValue();

        renderWithProviders(
            <SignalRContext.Provider
                value={{ connection: connection as never, isConnected: true }}
            >
                <LogsheetEventHandler />
            </SignalRContext.Provider>,
            { queryClient },
        );

        await act(async () => {
            await handlers.LogsheetAutomaticAlignmentFinished?.({
                logsheetId: "logsheet-3",
                isSuccess: false,
                errorMessage: "Could not detect markers",
            });
        });

        expect(toast.error).toHaveBeenCalledWith(
            "Logsheet automatic alignment failed",
            {
                id: "logsheet-automatic-alignment-error-logsheet-3",
                description: "Could not detect markers",
            },
        );
        expect(invalidateQueriesSpy).toHaveBeenCalledWith({
            queryKey: ["logsheets"],
        });
    });

    it("unsubscribes registered handlers on unmount", async () => {
        const handlers: HandlerMap = {};
        const connection = createConnection(handlers);

        const { unmount } = renderWithProviders(
            <SignalRContext.Provider
                value={{ connection: connection as never, isConnected: true }}
            >
                <LogsheetEventHandler />
            </SignalRContext.Provider>,
        );

        unmount();

        await waitFor(() => {
            expect(connection.off).toHaveBeenCalledWith(
                "LogsheetProcessingFinished",
                expect.any(Function),
            );
            expect(connection.off).toHaveBeenCalledWith(
                "LogsheetAutomaticAlignmentFinished",
                expect.any(Function),
            );
        });
        expect(handlers.LogsheetProcessingFinished).toBeUndefined();
        expect(handlers.LogsheetAutomaticAlignmentFinished).toBeUndefined();
    });
});
