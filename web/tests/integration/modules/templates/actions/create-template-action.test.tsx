import { CreateTemplateAction } from "@/modules/templates/actions/create-template-action/create-template-action";
import { renderWithProviders } from "../../../../utils/render-with-providers";
import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

vi.mock(
    "@/modules/templates/actions/create-template-action/create-template-wizard",
    () => ({
        CreateTemplateWizard: ({ onClose }: { onClose: () => void }) => (
            <div data-testid="create-template-wizard-stub">
                <button type="button" onClick={onClose}>
                    Close Wizard
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

describe("CreateTemplateAction", () => {
    it("opens dialog and renders wizard after clicking action button", async () => {
        const user = userEvent.setup();

        renderWithProviders(<CreateTemplateAction />);

        expect(
            screen.queryByTestId("create-template-wizard-stub"),
        ).not.toBeInTheDocument();

        await user.click(
            screen.getByRole("button", { name: "Create template" }),
        );

        expect(
            screen.getByTestId("create-template-wizard-stub"),
        ).toBeInTheDocument();
    });

    it("closes dialog when wizard calls onClose", async () => {
        const user = userEvent.setup();

        renderWithProviders(<CreateTemplateAction />);

        await user.click(
            screen.getByRole("button", { name: "Create template" }),
        );

        await user.click(screen.getByRole("button", { name: "Close Wizard" }));

        expect(
            screen.queryByTestId("create-template-wizard-stub"),
        ).not.toBeInTheDocument();
    });
});
