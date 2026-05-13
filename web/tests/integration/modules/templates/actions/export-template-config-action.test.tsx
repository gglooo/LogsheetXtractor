import { ExportTemplateConfigAction } from "@/modules/templates/actions/export-template-config-action";
import { renderWithProviders } from "../../../../utils/render-with-providers";
import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import type { ReactNode } from "react";
import { toast } from "sonner";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

const useExportConfigMutationMock = vi.fn();

vi.mock("@/modules/templates/api", () => ({
    useExportConfigMutation: () => useExportConfigMutationMock(),
}));

vi.mock("@/components/ui/dropdown-menu", () => ({
    DropdownMenuItem: ({
        children,
        onClick,
    }: {
        children: ReactNode;
        onClick?: () => void;
    }) => (
        <button type="button" onClick={onClick}>
            {children}
        </button>
    ),
    DropdownMenuSub: ({ children }: { children: ReactNode }) => (
        <div>{children}</div>
    ),
    DropdownMenuSubContent: ({ children }: { children: ReactNode }) => (
        <div>{children}</div>
    ),
    DropdownMenuSubTrigger: ({ children }: { children: ReactNode }) => (
        <div>{children}</div>
    ),
}));

vi.mock("sonner", () => ({
    toast: {
        success: vi.fn(),
        error: vi.fn(),
    },
}));

beforeEach(() => {
    localStorage.setItem("app-locale", "en");
    useExportConfigMutationMock.mockReturnValue({
        mutateAsync: vi.fn().mockResolvedValue(undefined),
    });
});

afterEach(() => {
    vi.clearAllMocks();
});

describe("ExportTemplateConfigAction", () => {
    it("exports frontside config with and without ROI validations", async () => {
        const user = userEvent.setup();
        const mutateAsync = vi.fn().mockResolvedValue(undefined);
        useExportConfigMutationMock.mockReturnValue({ mutateAsync });

        renderWithProviders(
            <ExportTemplateConfigAction
                templateId="front-template"
                backsideTemplateId={null}
            />,
        );

        await user.click(
            screen.getByRole("button", {
                name: "With ROI validations (incompatible with formHTR)",
            }),
        );
        await user.click(
            screen.getByRole("button", {
                name: "Without ROI validations",
            }),
        );

        await waitFor(() => {
            expect(mutateAsync).toHaveBeenCalledWith({
                templateId: "front-template",
                includeRoiValidations: true,
            });
            expect(mutateAsync).toHaveBeenCalledWith({
                templateId: "front-template",
                includeRoiValidations: false,
            });
            expect(toast.success).toHaveBeenCalledTimes(2);
        });
    });

    it("offers separate frontside and backside export targets", async () => {
        const user = userEvent.setup();
        const mutateAsync = vi.fn().mockResolvedValue(undefined);
        useExportConfigMutationMock.mockReturnValue({ mutateAsync });

        renderWithProviders(
            <ExportTemplateConfigAction
                templateId="front-template"
                backsideTemplateId="back-template"
            />,
        );

        expect(screen.getByText("Frontside")).toBeInTheDocument();
        expect(screen.getByText("Backside")).toBeInTheDocument();

        await user.click(
            screen.getAllByRole("button", {
                name: "Without ROI validations",
            })[1],
        );

        await waitFor(() => {
            expect(mutateAsync).toHaveBeenCalledWith({
                templateId: "back-template",
                includeRoiValidations: false,
            });
        });
    });

    it("shows an error toast when export fails", async () => {
        const user = userEvent.setup();
        useExportConfigMutationMock.mockReturnValue({
            mutateAsync: vi.fn().mockRejectedValue(new Error("export failed")),
        });

        renderWithProviders(
            <ExportTemplateConfigAction
                templateId="front-template"
                backsideTemplateId={null}
            />,
        );

        await user.click(
            screen.getByRole("button", {
                name: "Without ROI validations",
            }),
        );

        await waitFor(() => {
            expect(toast.error).toHaveBeenCalled();
        });
    });
});
