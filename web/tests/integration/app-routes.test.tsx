import App from "@/App";
import { renderWithProviders } from "../utils/render-with-providers";
import { screen } from "@testing-library/react";
import type React from "react";
import { describe, expect, it, vi } from "vitest";

vi.mock("@/modules/dashboard/routes", () => ({
    baseDashboardPath: "/dashboard",
    DashboardRoutes: () => <div>Dashboard route</div>,
}));

vi.mock("@/modules/settings/page", () => ({
    SettingsPage: () => <div>Settings route</div>,
}));

vi.mock("@/modules/template-editor/routes", () => ({
    baseTemplateEditorPath: "/template-editor",
    TemplateEditorRoutes: () => <div>Template editor route</div>,
}));

vi.mock("@/modules/logsheets/routes", () => ({
    baseLogsheetsPath: "/logsheets",
    LogsheetsRoutes: () => <div>Logsheets route</div>,
}));

vi.mock("@/modules/logsheets/proofreading/gamified-proofreading-page", () => ({
    GamifiedProofreadingPage: () => <div>Gamified route</div>,
}));

vi.mock("@/modules/signalr/signalr-provider", () => ({
    SignalRProvider: ({ children }: { children: React.ReactNode }) => (
        <>{children}</>
    ),
}));

vi.mock("@/modules/logsheets/logsheet-event-handler", () => ({
    LogsheetEventHandler: () => null,
}));

describe("App routes", () => {
    it("redirects the root route to dashboard", async () => {
        renderWithProviders(<App />, { route: "/" });

        expect(await screen.findByText("Dashboard route")).toBeInTheDocument();
    });

    it("renders not found fallback for unknown routes", () => {
        renderWithProviders(<App />, { route: "/unknown" });

        expect(screen.getByText("404 - Not Found")).toBeInTheDocument();
    });
});
