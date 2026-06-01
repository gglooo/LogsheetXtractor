import { DetectRoisAction } from "@/modules/rois/actions/detect-rois-action";
import type { DetectRoiResponseType } from "@/modules/rois/schema";
import { renderWithProviders } from "../../../../utils/render-with-providers";
import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { toast } from "sonner";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

const useDetectRoisMutationMock = vi.fn();

vi.mock("@/modules/rois/api", () => ({
    useDetectRoisMutation: () => useDetectRoisMutationMock(),
}));

vi.mock("sonner", () => ({
    toast: {
        success: vi.fn(),
        error: vi.fn(),
    },
}));

beforeEach(() => {
    localStorage.setItem("app-locale", "en");
    useDetectRoisMutationMock.mockReturnValue({
        mutateAsync: vi.fn().mockResolvedValue({
            rois: [],
            residuals: [],
        } satisfies DetectRoiResponseType),
        isPending: false,
    });
});

afterEach(() => {
    vi.clearAllMocks();
});

describe("DetectRoisAction", () => {
    it("runs detection, passes the result to the caller, and shows success toast", async () => {
        const user = userEvent.setup();
        const detected = {
            rois: [],
            residuals: [],
        } satisfies DetectRoiResponseType;
        const mutateAsync = vi.fn().mockResolvedValue(detected);
        const onResult = vi.fn();

        useDetectRoisMutationMock.mockReturnValue({
            mutateAsync,
            isPending: false,
        });

        renderWithProviders(
            <DetectRoisAction templateId="tpl-123" onResult={onResult} />,
        );

        await user.click(screen.getByRole("button", { name: "Detect ROIs" }));

        await waitFor(() => {
            expect(mutateAsync).toHaveBeenCalledWith("tpl-123");
            expect(onResult).toHaveBeenCalledWith(detected);
            expect(toast.success).toHaveBeenCalled();
        });
    });

    it("shows an error toast when detection fails", async () => {
        const user = userEvent.setup();
        useDetectRoisMutationMock.mockReturnValue({
            mutateAsync: vi.fn().mockRejectedValue(new Error("detect failed")),
            isPending: false,
        });

        renderWithProviders(<DetectRoisAction templateId="tpl-123" />);

        await user.click(screen.getByRole("button", { name: "Detect ROIs" }));

        await waitFor(() => {
            expect(toast.error).toHaveBeenCalled();
        });
    });

    it("keeps the action disabled when requested", () => {
        renderWithProviders(<DetectRoisAction templateId="tpl-123" disabled />);

        expect(
            screen.getByRole("button", { name: "Detect ROIs" }),
        ).toBeDisabled();
    });
});
