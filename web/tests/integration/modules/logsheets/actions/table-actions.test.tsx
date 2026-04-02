import { LogsheetTableActions } from "@/modules/logsheets/actions/table-actions";
import type { LogsheetListType } from "@/modules/logsheets/schema";
import { renderWithProviders } from "../../../../utils/render-with-providers";
import { waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { screen } from "@testing-library/react";
import { toast } from "sonner";

const mockNavigate = vi.fn();
const mockUseParams = vi.fn(() => ({ templateId: "template-123" }));

const useProcessLogsheetMutationMock = vi.fn();
const useDeleteLogsheetMutationMock = vi.fn();
const useExportLogsheetMutationMock = vi.fn();
const useFileDownloadMutationMock = vi.fn();
const useCredentialsStatusMock = vi.fn();

vi.mock("react-router-dom", async () => {
    const actual = await vi.importActual<typeof import("react-router-dom")>(
        "react-router-dom",
    );

    return {
        ...actual,
        useNavigate: () => mockNavigate,
        useParams: () => mockUseParams(),
    };
});

vi.mock("@/modules/logsheets/api", () => ({
    useProcessLogsheetMutation: () => useProcessLogsheetMutationMock(),
    useDeleteLogsheetMutation: () => useDeleteLogsheetMutationMock(),
    useExportLogsheetMutation: () => useExportLogsheetMutationMock(),
}));

vi.mock("@/modules/files/api", () => ({
    useFileDownloadMutation: () => useFileDownloadMutationMock(),
}));

vi.mock("@/modules/settings/api", () => ({
    useCredentialsStatus: () => useCredentialsStatusMock(),
}));

vi.mock("sonner", () => ({
    toast: {
        success: vi.fn(),
        error: vi.fn(),
    },
}));

const now = new Date().toISOString();

const createLogsheet = (status: LogsheetListType["status"]): LogsheetListType => ({
    id: "11111111-1111-4111-8111-111111111111",
    createdAt: now,
    updatedAt: null,
    deletedAt: null,
    templateId: "22222222-2222-4222-8222-222222222222",
    file: {
        id: "33333333-3333-4333-8333-333333333333",
        createdAt: now,
        updatedAt: null,
        deletedAt: null,
        fileName: "logsheet.pdf",
        contentType: "application/pdf",
        sizeBytes: 10,
    },
    status,
    processedAt: null,
    isFrontAligned: false,
    isBackAligned: false,
    errorMessage: null,
});

beforeEach(() => {
    localStorage.setItem("app-locale", "en");

    useProcessLogsheetMutationMock.mockReturnValue({
        mutateAsync: vi.fn().mockResolvedValue(undefined),
        isPending: false,
    });

    useDeleteLogsheetMutationMock.mockReturnValue({
        mutateAsync: vi.fn().mockResolvedValue(undefined),
        isPending: false,
    });

    useExportLogsheetMutationMock.mockReturnValue({
        mutateAsync: vi.fn().mockResolvedValue(undefined),
        isPending: false,
    });

    useFileDownloadMutationMock.mockReturnValue({
        mutateAsync: vi.fn().mockResolvedValue(undefined),
        isPending: false,
    });

    useCredentialsStatusMock.mockReturnValue({
        data: { available: true, hasUserCredentials: true },
    });
});

afterEach(() => {
    vi.clearAllMocks();
});

describe("LogsheetTableActions", () => {
    it("calls onPreview callback when preview button is clicked", async () => {
        const user = userEvent.setup();
        const onPreview = vi.fn();

        renderWithProviders(
            <LogsheetTableActions
                logsheet={createLogsheet("Pending")}
                onPreview={onPreview}
            />,
        );

        await user.click(screen.getByTitle("Preview"));

        expect(onPreview).toHaveBeenCalledWith(
            "11111111-1111-4111-8111-111111111111",
        );
    });

    it("processes logsheet and shows success toast", async () => {
        const user = userEvent.setup();
        const mutateAsync = vi.fn().mockResolvedValue(undefined);
        useProcessLogsheetMutationMock.mockReturnValue({
            mutateAsync,
            isPending: false,
        });

        renderWithProviders(
            <LogsheetTableActions
                logsheet={createLogsheet("Pending")}
                onPreview={vi.fn()}
            />,
        );

        await user.click(screen.getByTitle("Process"));

        await waitFor(() => {
            expect(mutateAsync).toHaveBeenCalledWith(
                "11111111-1111-4111-8111-111111111111",
            );
            expect(toast.success).toHaveBeenCalledWith(
                "Logsheet was queued for processing.",
            );
        });
    });

    it("disables process button when credentials are missing", () => {
        useCredentialsStatusMock.mockReturnValue({
            data: { available: false, hasUserCredentials: false },
        });

        renderWithProviders(
            <LogsheetTableActions
                logsheet={createLogsheet("Pending")}
                onPreview={vi.fn()}
            />,
        );

        expect(screen.getByTitle("Process")).toBeDisabled();
    });

    it("navigates to proofread route when proofread button is clicked", async () => {
        const user = userEvent.setup();

        renderWithProviders(
            <LogsheetTableActions
                logsheet={createLogsheet("NeedsReview")}
                onPreview={vi.fn()}
            />,
        );

        await user.click(screen.getByTitle("Proofread"));

        expect(mockNavigate).toHaveBeenCalledWith(
            "/templates/template-123/logsheets/11111111-1111-4111-8111-111111111111/proofread",
        );
    });

    it("shows error toast when process fails", async () => {
        const user = userEvent.setup();
        const mutateAsync = vi.fn().mockRejectedValue(new Error("boom"));
        useProcessLogsheetMutationMock.mockReturnValue({
            mutateAsync,
            isPending: false,
        });

        renderWithProviders(
            <LogsheetTableActions
                logsheet={createLogsheet("Pending")}
                onPreview={vi.fn()}
            />,
        );

        await user.click(screen.getByTitle("Process"));

        await waitFor(() => {
            expect(mutateAsync).toHaveBeenCalledWith(
                "11111111-1111-4111-8111-111111111111",
            );
            expect(toast.error).toHaveBeenCalledWith(
                "Failed to queue logsheet for processing.",
            );
        });
    });

    it("hides process action when logsheet state cannot be processed", () => {
        renderWithProviders(
            <LogsheetTableActions
                logsheet={createLogsheet("Completed")}
                onPreview={vi.fn()}
            />,
        );

        expect(screen.queryByTitle("Process")).not.toBeInTheDocument();
    });

    it("disables export action when logsheet state cannot be exported", async () => {
        const user = userEvent.setup();

        renderWithProviders(
            <LogsheetTableActions
                logsheet={createLogsheet("Pending")}
                onPreview={vi.fn()}
            />,
        );

        await user.click(screen.getByTitle("More actions"));

        expect(
            screen.getByRole("menuitem", { name: "Export proofreading data" }),
        ).toHaveAttribute("aria-disabled", "true");
    });

    it("exports completed logsheet from menu action", async () => {
        const user = userEvent.setup();
        const mutateAsync = vi.fn().mockResolvedValue(undefined);
        useExportLogsheetMutationMock.mockReturnValue({
            mutateAsync,
            isPending: false,
        });

        renderWithProviders(
            <LogsheetTableActions
                logsheet={createLogsheet("Completed")}
                onPreview={vi.fn()}
            />,
        );

        await user.click(screen.getByTitle("More actions"));
        await user.click(
            screen.getByRole("menuitem", { name: "Export proofreading data" }),
        );

        await waitFor(() => {
            expect(mutateAsync).toHaveBeenCalledWith({
                logsheetId: "11111111-1111-4111-8111-111111111111",
            });
        });
    });

    it("disables delete action when logsheet state cannot be deleted", async () => {
        const user = userEvent.setup();

        renderWithProviders(
            <LogsheetTableActions
                logsheet={createLogsheet("Processing")}
                onPreview={vi.fn()}
            />,
        );

        await user.click(screen.getByTitle("More actions"));

        expect(screen.getByRole("menuitem", { name: "Delete" })).toHaveAttribute(
            "aria-disabled",
            "true",
        );
    });

    it("deletes logsheet from menu action and shows success toast", async () => {
        const user = userEvent.setup();
        const mutateAsync = vi.fn().mockResolvedValue(undefined);
        useDeleteLogsheetMutationMock.mockReturnValue({
            mutateAsync,
            isPending: false,
        });

        renderWithProviders(
            <LogsheetTableActions
                logsheet={createLogsheet("Pending")}
                onPreview={vi.fn()}
            />,
        );

        await user.click(screen.getByTitle("More actions"));
        await user.click(screen.getByRole("menuitem", { name: "Delete" }));

        await waitFor(() => {
            expect(mutateAsync).toHaveBeenCalledWith(
                "11111111-1111-4111-8111-111111111111",
            );
            expect(toast.success).toHaveBeenCalledWith("Logsheet was deleted.");
        });
    });
});
