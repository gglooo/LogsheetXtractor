import { LogsheetTableBulkActions } from "@/modules/logsheets/table/bulk-actions";
import { renderWithProviders } from "../../../../utils/render-with-providers";
import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { toast } from "sonner";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

const useDeleteLogsheetsMutationMock = vi.fn();
const useProcessLogsheetsMutationMock = vi.fn();
const useExportLogsheetsMutationMock = vi.fn();

vi.mock("@/modules/logsheets/api", () => ({
    useDeleteLogsheetsMutation: () => useDeleteLogsheetsMutationMock(),
    useProcessLogsheetsMutation: () => useProcessLogsheetsMutationMock(),
    useExportLogsheetsMutation: () => useExportLogsheetsMutationMock(),
}));

vi.mock("sonner", () => ({
    toast: {
        success: vi.fn(),
        error: vi.fn(),
    },
}));

beforeEach(() => {
    localStorage.setItem("app-locale", "en");

    useDeleteLogsheetsMutationMock.mockReturnValue({
        mutateAsync: vi.fn().mockResolvedValue(undefined),
        isPending: false,
    });
    useProcessLogsheetsMutationMock.mockReturnValue({
        mutateAsync: vi.fn().mockResolvedValue(undefined),
        isPending: false,
    });
    useExportLogsheetsMutationMock.mockReturnValue({
        mutateAsync: vi.fn().mockResolvedValue(undefined),
        isPending: false,
    });
});

afterEach(() => {
    vi.clearAllMocks();
});

describe("LogsheetTableBulkActions", () => {
    it("does not render when no logsheets are selected", () => {
        renderWithProviders(
            <LogsheetTableBulkActions
                selectedLogsheetIds={[]}
                onClearSelection={vi.fn()}
            />,
        );

        expect(
            screen.queryByText(/logsheet\(s\) selected/i),
        ).not.toBeInTheDocument();
    });

    it("processes selected logsheets and clears selection on success", async () => {
        const user = userEvent.setup();
        const mutateAsync = vi.fn().mockResolvedValue(undefined);
        const onClearSelection = vi.fn();

        useProcessLogsheetsMutationMock.mockReturnValue({
            mutateAsync,
            isPending: false,
        });

        renderWithProviders(
            <LogsheetTableBulkActions
                selectedLogsheetIds={["a", "b"]}
                onClearSelection={onClearSelection}
            />,
        );

        await user.click(screen.getByRole("button", { name: "Process" }));

        await waitFor(() => {
            expect(mutateAsync).toHaveBeenCalledWith(["a", "b"]);
            expect(onClearSelection).toHaveBeenCalled();
            expect(toast.success).toHaveBeenCalled();
        });
    });

    it("shows error toast when delete fails and keeps selection", async () => {
        const user = userEvent.setup();
        const mutateAsync = vi.fn().mockRejectedValue(new Error("delete failed"));
        const onClearSelection = vi.fn();

        useDeleteLogsheetsMutationMock.mockReturnValue({
            mutateAsync,
            isPending: false,
        });

        renderWithProviders(
            <LogsheetTableBulkActions
                selectedLogsheetIds={["a", "b"]}
                onClearSelection={onClearSelection}
            />,
        );

        await user.click(screen.getByRole("button", { name: "Delete" }));

        await waitFor(() => {
            expect(mutateAsync).toHaveBeenCalledWith(["a", "b"]);
            expect(onClearSelection).not.toHaveBeenCalled();
            expect(toast.error).toHaveBeenCalled();
        });
    });

    it("exports selected logsheets and clears selection on success", async () => {
        const user = userEvent.setup();
        const mutateAsync = vi.fn().mockResolvedValue(undefined);
        const onClearSelection = vi.fn();

        useExportLogsheetsMutationMock.mockReturnValue({
            mutateAsync,
            isPending: false,
        });

        renderWithProviders(
            <LogsheetTableBulkActions
                selectedLogsheetIds={["a", "b", "c"]}
                onClearSelection={onClearSelection}
            />,
        );

        await user.click(
            screen.getByRole("button", { name: "Export proofreading data" }),
        );

        await waitFor(() => {
            expect(mutateAsync).toHaveBeenCalledWith(["a", "b", "c"]);
            expect(onClearSelection).toHaveBeenCalled();
            expect(toast.success).toHaveBeenCalled();
        });
    });

    it("shows error toast when export fails and keeps selection", async () => {
        const user = userEvent.setup();
        const mutateAsync = vi.fn().mockRejectedValue(new Error("export failed"));
        const onClearSelection = vi.fn();

        useExportLogsheetsMutationMock.mockReturnValue({
            mutateAsync,
            isPending: false,
        });

        renderWithProviders(
            <LogsheetTableBulkActions
                selectedLogsheetIds={["a", "b", "c"]}
                onClearSelection={onClearSelection}
            />,
        );

        await user.click(
            screen.getByRole("button", { name: "Export proofreading data" }),
        );

        await waitFor(() => {
            expect(mutateAsync).toHaveBeenCalledWith(["a", "b", "c"]);
            expect(onClearSelection).not.toHaveBeenCalled();
            expect(toast.error).toHaveBeenCalled();
        });
    });

    it("disables export button and shows exporting dialog while export is pending", () => {
        useExportLogsheetsMutationMock.mockReturnValue({
            mutateAsync: vi.fn(),
            isPending: true,
        });

        renderWithProviders(
            <LogsheetTableBulkActions
                selectedLogsheetIds={["a", "b", "c"]}
                onClearSelection={vi.fn()}
            />,
        );

        expect(
            screen
                .getByText("Export proofreading data", { selector: "button" })
                .closest("button"),
        ).toBeDisabled();
        expect(screen.getByText("Exporting logsheets")).toBeInTheDocument();
    });

    it("disables process button and shows processing dialog while process is pending", () => {
        useProcessLogsheetsMutationMock.mockReturnValue({
            mutateAsync: vi.fn(),
            isPending: true,
        });

        renderWithProviders(
            <LogsheetTableBulkActions
                selectedLogsheetIds={["a", "b", "c"]}
                onClearSelection={vi.fn()}
            />,
        );

        expect(
            screen.getByText("Process", { selector: "button" }).closest("button"),
        ).toBeDisabled();
        expect(screen.getByText("Processing logsheets")).toBeInTheDocument();
    });
});
