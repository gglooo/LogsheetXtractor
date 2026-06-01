import { LogsheetsPage } from "@/modules/logsheets/page";
import { baseTemplateEditorPath } from "@/modules/template-editor/routes";
import { renderWithProviders } from "../../../utils/render-with-providers";
import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

const mockNavigate = vi.fn();
const useParamsMock = vi.fn();

vi.mock("@/modules/logsheets/components/navbar", () => ({
    LogsheetsNavbar: () => <div data-testid="logsheets-navbar" />,
}));

vi.mock("@/modules/logsheets/table/logsheet-table", () => ({
    LogsheetTable: ({ templateId }: { templateId: string }) => (
        <div data-testid="logsheet-table">table:{templateId}</div>
    ),
}));

vi.mock("react-router-dom", async () => {
    const actual = await vi.importActual<typeof import("react-router-dom")>(
        "react-router-dom",
    );

    return {
        ...actual,
        useNavigate: () => mockNavigate,
        useParams: () => useParamsMock(),
    };
});

beforeEach(() => {
    localStorage.setItem("app-locale", "en");
    useParamsMock.mockReturnValue({ templateId: "tpl-1" });
});

afterEach(() => {
    vi.clearAllMocks();
});

describe("LogsheetsPage", () => {
    it("shows fallback when template id is missing", () => {
        useParamsMock.mockReturnValue({ templateId: undefined });

        renderWithProviders(<LogsheetsPage />);

        expect(screen.getByText("No template ID provided.")).toBeInTheDocument();
    });

    it("navigates to upload and edit template routes", async () => {
        const user = userEvent.setup();

        renderWithProviders(<LogsheetsPage />);

        expect(screen.getByTestId("logsheets-navbar")).toBeInTheDocument();
        expect(screen.getByTestId("logsheet-table")).toHaveTextContent(
            "table:tpl-1",
        );

        await user.click(
            screen.getByRole("button", { name: "Add logsheets" }),
        );
        await user.click(
            screen.getByRole("button", { name: "Edit template" }),
        );

        expect(mockNavigate).toHaveBeenNthCalledWith(1, "upload");
        expect(mockNavigate).toHaveBeenNthCalledWith(
            2,
            `${baseTemplateEditorPath}/tpl-1`,
        );
    });
});
