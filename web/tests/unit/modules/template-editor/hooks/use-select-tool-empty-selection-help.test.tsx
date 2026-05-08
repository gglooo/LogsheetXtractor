import { useSelectToolEmptySelectionHelp } from "@/modules/template-editor/hooks/use-select-tool-empty-selection-help";
import { renderHook, act } from "@testing-library/react";
import { IntlProvider } from "react-intl";
import { toast } from "sonner";
import { afterEach, describe, expect, it, vi } from "vitest";

vi.mock("sonner", () => ({
    toast: {
        warning: vi.fn(),
    },
}));

const Wrapper = ({ children }: { children: React.ReactNode }) => (
    <IntlProvider locale="en">{children}</IntlProvider>
);

describe("useSelectToolEmptySelectionHelp", () => {
    afterEach(() => {
        vi.clearAllMocks();
    });

    it("shows warning on second empty draw finish in select mode", () => {
        const { result } = renderHook(
            () => useSelectToolEmptySelectionHelp({ mode: "select" }),
            { wrapper: Wrapper },
        );

        act(() => {
            result.current.trackSelectionResult(0);
        });
        expect(toast.warning).not.toHaveBeenCalled();

        act(() => {
            result.current.trackSelectionResult(0);
        });
        expect(toast.warning).toHaveBeenCalledTimes(1);
    });

    it("does not spam warning after it has been shown", () => {
        const { result } = renderHook(
            () => useSelectToolEmptySelectionHelp({ mode: "select" }),
            { wrapper: Wrapper },
        );

        act(() => {
            result.current.trackSelectionResult(0);
            result.current.trackSelectionResult(0);
            result.current.trackSelectionResult(0);
        });

        expect(toast.warning).toHaveBeenCalledTimes(1);
    });

    it("resets attempts after a non-empty finish", () => {
        const { result } = renderHook(
            () => useSelectToolEmptySelectionHelp({ mode: "select" }),
            { wrapper: Wrapper },
        );

        act(() => {
            result.current.trackSelectionResult(0);
            result.current.trackSelectionResult(0);
            result.current.trackSelectionResult(2);
            result.current.trackSelectionResult(0);
            result.current.trackSelectionResult(0);
        });

        expect(toast.warning).toHaveBeenCalledTimes(2);
    });

    it("resets attempts when mode changes away from select", () => {
        const { result, rerender } = renderHook(
            ({ mode }: { mode: "select" | "draw" | "split" }) =>
                useSelectToolEmptySelectionHelp({ mode }),
            {
                initialProps: { mode: "select" as const },
                wrapper: Wrapper,
            },
        );

        act(() => {
            result.current.trackSelectionResult(0);
            result.current.trackSelectionResult(0);
        });
        expect(toast.warning).toHaveBeenCalledTimes(1);

        rerender({ mode: "draw" });
        rerender({ mode: "select" });

        act(() => {
            result.current.trackSelectionResult(0);
            result.current.trackSelectionResult(0);
        });

        expect(toast.warning).toHaveBeenCalledTimes(2);
    });
});
