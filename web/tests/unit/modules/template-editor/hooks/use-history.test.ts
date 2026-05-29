import { renderHook, act } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { useHistory } from "@/modules/template-editor/hooks/use-history";

describe("useHistory", () => {
    it("tracks set, undo, and redo", () => {
        const { result } = renderHook(() => useHistory<number>(0));

        act(() => {
            result.current.set(1);
            result.current.set(2);
        });

        expect(result.current.state).toBe(2);
        expect(result.current.canUndo).toBe(true);
        expect(result.current.canRedo).toBe(false);

        act(() => {
            result.current.undo();
        });

        expect(result.current.state).toBe(1);
        expect(result.current.canRedo).toBe(true);

        act(() => {
            result.current.redo();
        });

        expect(result.current.state).toBe(2);
    });

    it("supports functional set updates", () => {
        const { result } = renderHook(() => useHistory<number>(5));

        act(() => {
            result.current.set((prev) => prev + 3);
        });

        expect(result.current.state).toBe(8);
    });

    it("respects history depth limit", () => {
        const { result } = renderHook(() => useHistory<number>(0, 2));

        act(() => {
            result.current.set(1);
            result.current.set(2);
            result.current.set(3);
        });

        act(() => {
            result.current.undo();
            result.current.undo();
            result.current.undo();
        });

        expect(result.current.state).toBe(1);
    });

    it("tracks dirty flag and markAsSaved", () => {
        const { result } = renderHook(() => useHistory<{ value: number }>({ value: 1 }));

        expect(result.current.isDirty).toBe(false);

        act(() => {
            result.current.set({ value: 2 });
        });

        expect(result.current.isDirty).toBe(true);

        act(() => {
            result.current.markAsSaved();
        });

        expect(result.current.isDirty).toBe(false);
    });
});
