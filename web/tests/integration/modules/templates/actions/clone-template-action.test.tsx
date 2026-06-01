import { CloneTemplateAction } from "@/modules/templates/actions/clone-template-action";
import { renderWithProviders } from "../../../../utils/render-with-providers";
import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import type { ReactNode } from "react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

vi.mock("@/components/ui/dropdown-menu", () => ({
    DropdownMenuItem: ({
        children,
        onSelect,
    }: {
        children: ReactNode;
        onSelect?: (e: { preventDefault: () => void }) => void;
    }) => (
        <button
            type="button"
            onClick={() =>
                onSelect?.({
                    preventDefault: () => undefined,
                })
            }
        >
            {children}
        </button>
    ),
}));

vi.mock(
    "@/modules/templates/actions/clone-template-action/clone-template-wizard",
    () => ({
        CloneTemplateWizard: ({
            onClose,
            templateId,
        }: {
            onClose: () => void;
            templateId: string;
        }) => (
            <div data-testid="clone-template-wizard-stub">
                <span data-testid="clone-template-id">{templateId}</span>
                <button type="button" onClick={onClose}>
                    Close Clone Wizard
                </button>
            </div>
        ),
    }),
);

beforeEach(() => {
    localStorage.setItem("app-locale", "en");
});

afterEach(() => {
    vi.clearAllMocks();
});

describe("CloneTemplateAction", () => {
    it("opens dialog and passes template id to wizard", async () => {
        const user = userEvent.setup();

        renderWithProviders(
            <CloneTemplateAction templateId="tpl-123" />,
        );

        expect(
            screen.queryByTestId("clone-template-wizard-stub"),
        ).not.toBeInTheDocument();

        await user.click(screen.getByRole("button", { name: "Clone Template" }));

        expect(
            screen.getByTestId("clone-template-wizard-stub"),
        ).toBeInTheDocument();
        expect(screen.getByTestId("clone-template-id")).toHaveTextContent(
            "tpl-123",
        );
    });

    it("closes dialog when wizard calls onClose", async () => {
        const user = userEvent.setup();

        renderWithProviders(
            <CloneTemplateAction templateId="tpl-123" />,
        );

        await user.click(screen.getByRole("button", { name: "Clone Template" }));
        await user.click(
            screen.getByRole("button", { name: "Close Clone Wizard" }),
        );

        expect(
            screen.queryByTestId("clone-template-wizard-stub"),
        ).not.toBeInTheDocument();
    });
});
