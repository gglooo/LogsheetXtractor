import { CredentialsAction } from "@/modules/settings/actions/credentials-action";
import { renderWithProviders } from "../../../../utils/render-with-providers";
import { waitFor, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { toast } from "sonner";

const useCredentialsStatusMock = vi.fn();
const useSetCredentialsMutationMock = vi.fn();

vi.mock("@/modules/settings/api", () => ({
    useCredentialsStatus: () => useCredentialsStatusMock(),
    useSetCredentialsMutation: () => useSetCredentialsMutationMock(),
}));

vi.mock("sonner", () => ({
    toast: {
        success: vi.fn(),
        error: vi.fn(),
    },
}));

beforeEach(() => {
    localStorage.setItem("app-locale", "en");

    useCredentialsStatusMock.mockReturnValue({
        isLoading: false,
    });

    useSetCredentialsMutationMock.mockReturnValue({
        mutateAsync: vi.fn().mockResolvedValue(undefined),
        isPending: false,
    });
});

afterEach(() => {
    vi.clearAllMocks();
});

describe("CredentialsAction", () => {
    it("shows loading spinner while credentials status is loading", () => {
        useCredentialsStatusMock.mockReturnValue({
            isLoading: true,
        });

        renderWithProviders(<CredentialsAction />);

        expect(screen.getByRole("status", { name: "Loading" })).toBeInTheDocument();
        expect(screen.queryByText("Save credentials")).not.toBeInTheDocument();
    });

    it("submits default values and shows success toast", async () => {
        const user = userEvent.setup();
        const mutateAsync = vi.fn().mockResolvedValue(undefined);
        useSetCredentialsMutationMock.mockReturnValue({
            mutateAsync,
            isPending: false,
        });

        renderWithProviders(<CredentialsAction />);

        await user.click(screen.getByRole("button", { name: "Save credentials" }));

        await waitFor(() => {
            expect(mutateAsync).toHaveBeenCalledWith({
                keys: {
                    Google: "",
                    Azure: "",
                    Amazon: "",
                },
            });
            expect(toast.success).toHaveBeenCalled();
        });
    });

    it("shows error toast when saving credentials fails", async () => {
        const user = userEvent.setup();
        const mutateAsync = vi.fn().mockRejectedValue(new Error("boom"));
        useSetCredentialsMutationMock.mockReturnValue({
            mutateAsync,
            isPending: false,
        });

        renderWithProviders(<CredentialsAction />);

        await user.click(screen.getByRole("button", { name: "Save credentials" }));

        await waitFor(() => {
            expect(toast.error).toHaveBeenCalled();
        });
    });
});
