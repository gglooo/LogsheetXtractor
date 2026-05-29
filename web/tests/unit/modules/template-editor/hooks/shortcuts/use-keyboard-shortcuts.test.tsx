import { useKeyboardShortcuts } from "@/modules/template-editor/hooks/shortcuts/use-keyboard-shortcuts";
import { renderHook } from "@testing-library/react";
import type React from "react";
import { IntlProvider } from "react-intl";
import { afterEach, describe, expect, it, vi } from "vitest";

const Wrapper = ({ children }: { children: React.ReactNode }) => (
    <IntlProvider locale="en">{children}</IntlProvider>
);

const createActions = () => ({
    select: vi.fn(),
    draw: vi.fn(),
    split: vi.fn(),
    clear: vi.fn(),
    undo: vi.fn(),
    redo: vi.fn(),
    selectAll: vi.fn(),
    delete: vi.fn(),
    copy: vi.fn(),
    paste: vi.fn(),
    cut: vi.fn(),
    browse: vi.fn(),
    focusRoiInput: vi.fn(),
});

describe("useKeyboardShortcuts", () => {
    afterEach(() => {
        vi.clearAllMocks();
        document.body.innerHTML = "";
    });

    it("dispatches registered shortcuts and prevents the browser default", () => {
        const actions = createActions();
        renderHook(() => useKeyboardShortcuts(actions), { wrapper: Wrapper });

        const event = new KeyboardEvent("keydown", {
            key: "z",
            ctrlKey: true,
            bubbles: true,
            cancelable: true,
        });
        window.dispatchEvent(event);

        expect(actions.undo).toHaveBeenCalledTimes(1);
        expect(event.defaultPrevented).toBe(true);
    });

    it("matches shifted shortcuts", () => {
        const actions = createActions();
        renderHook(() => useKeyboardShortcuts(actions), { wrapper: Wrapper });

        window.dispatchEvent(
            new KeyboardEvent("keydown", {
                key: "z",
                ctrlKey: true,
                shiftKey: true,
                bubbles: true,
                cancelable: true,
            }),
        );

        expect(actions.redo).toHaveBeenCalledTimes(1);
    });

    it("ignores text input shortcuts unless the focused element is whitelisted", () => {
        const actions = createActions();
        const input = document.createElement("input");
        input.id = "roi-name";
        document.body.append(input);
        input.focus();

        const { rerender } = renderHook(
            ({ whitelist }) => useKeyboardShortcuts(actions, whitelist),
            {
                initialProps: { whitelist: {} },
                wrapper: Wrapper,
            },
        );

        input.dispatchEvent(
            new KeyboardEvent("keydown", {
                key: "a",
                ctrlKey: true,
                bubbles: true,
                cancelable: true,
            }),
        );
        expect(actions.selectAll).not.toHaveBeenCalled();

        rerender({ whitelist: { "roi-name": ["Ctrl+a"] } });

        input.dispatchEvent(
            new KeyboardEvent("keydown", {
                key: "a",
                ctrlKey: true,
                bubbles: true,
                cancelable: true,
            }),
        );

        expect(actions.selectAll).toHaveBeenCalledTimes(1);
    });

    it("removes the listener on unmount", () => {
        const actions = createActions();
        const { unmount } = renderHook(() => useKeyboardShortcuts(actions), {
            wrapper: Wrapper,
        });

        unmount();
        window.dispatchEvent(new KeyboardEvent("keydown", { key: "d" }));

        expect(actions.draw).not.toHaveBeenCalled();
    });
});
