import { AddTemplateBacksideAction } from "@/modules/templates/actions/add-template-backside-action";
import { renderWithProviders } from "../../../../utils/render-with-providers";
import { screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { useFormContext } from "react-hook-form";
import { toast } from "sonner";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

const useUploadFileMutationMock = vi.fn();
const useAddTemplateBacksideMutationMock = vi.fn();

vi.mock("@/modules/files/api", () => ({
    useUploadFileMutation: () => useUploadFileMutationMock(),
}));

vi.mock("@/modules/templates/api", () => ({
    useAddTemplateBacksideMutation: () => useAddTemplateBacksideMutationMock(),
}));

vi.mock("@/modules/templates/actions/backside-template-form", () => ({
    BacksideTemplateForm: ({ fieldPrefix }: { fieldPrefix: string }) => {
        const { setValue } = useFormContext();
        return (
            <div data-testid="backside-form-stub">
                <button
                    type="button"
                    onClick={() =>
                        setValue(
                            `${fieldPrefix}.file`,
                            new File(["%PDF"], "back.pdf", {
                                type: "application/pdf",
                            }),
                            { shouldValidate: true },
                        )
                    }
                >
                    Set Backside File
                </button>
            </div>
        );
    },
}));

vi.mock("sonner", () => ({
    toast: {
        success: vi.fn(),
        error: vi.fn(),
    },
}));

beforeEach(() => {
    localStorage.setItem("app-locale", "en");

    useUploadFileMutationMock.mockReturnValue({
        mutateAsync: vi.fn().mockResolvedValue({
            id: "11111111-1111-4111-8111-111111111111",
        }),
        isPending: false,
    });

    useAddTemplateBacksideMutationMock.mockReturnValue({
        mutateAsync: vi.fn().mockResolvedValue(undefined),
        isPending: false,
    });
});

afterEach(() => {
    vi.clearAllMocks();
});

describe("AddTemplateBacksideAction", () => {
    it("opens dialog when action button is clicked", async () => {
        const user = userEvent.setup();

        renderWithProviders(
            <AddTemplateBacksideAction templateId="tpl-123" />,
        );

        await user.click(screen.getByRole("button", { name: "Add backside" }));

        expect(screen.getByTestId("backside-form-stub")).toBeInTheDocument();
        expect(
            screen.getByText("Add backside template"),
        ).toBeInTheDocument();
    });

    it("uploads file, adds backside template, and shows success toast", async () => {
        const user = userEvent.setup();
        const uploadMutateAsync = vi.fn().mockResolvedValue({
            id: "11111111-1111-4111-8111-111111111111",
        });
        const addBacksideMutateAsync = vi.fn().mockResolvedValue(undefined);

        useUploadFileMutationMock.mockReturnValue({
            mutateAsync: uploadMutateAsync,
            isPending: false,
        });

        useAddTemplateBacksideMutationMock.mockReturnValue({
            mutateAsync: addBacksideMutateAsync,
            isPending: false,
        });

        renderWithProviders(
            <AddTemplateBacksideAction templateId="tpl-123" />,
        );

        await user.click(screen.getByRole("button", { name: "Add backside" }));

        const dialog = screen.getByRole("dialog");
        await user.click(
            within(dialog).getByRole("button", { name: "Set Backside File" }),
        );
        await user.click(
            within(dialog).getAllByRole("button", { name: "Add backside" })[0],
        );

        await waitFor(() => {
            expect(uploadMutateAsync).toHaveBeenCalledTimes(1);
            expect(addBacksideMutateAsync).toHaveBeenCalledWith({
                templateId: "tpl-123",
                fileId: "11111111-1111-4111-8111-111111111111",
            });
            expect(toast.success).toHaveBeenCalled();
        });
    });

    it("shows error toast when upload or add fails", async () => {
        const user = userEvent.setup();
        const uploadMutateAsync = vi
            .fn()
            .mockRejectedValue(new Error("upload failed"));

        useUploadFileMutationMock.mockReturnValue({
            mutateAsync: uploadMutateAsync,
            isPending: false,
        });

        renderWithProviders(
            <AddTemplateBacksideAction templateId="tpl-123" />,
        );

        await user.click(screen.getByRole("button", { name: "Add backside" }));

        const dialog = screen.getByRole("dialog");
        await user.click(
            within(dialog).getByRole("button", { name: "Set Backside File" }),
        );
        await user.click(
            within(dialog).getAllByRole("button", { name: "Add backside" })[0],
        );

        await waitFor(() => {
            expect(toast.error).toHaveBeenCalled();
        });
    });
});
