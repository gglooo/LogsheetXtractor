import { useMouseZoom } from "@/modules/pdf/hooks/use-mouse-zoom";
import { renderHook } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

describe("useMouseZoom", () => {
    it("applies ctrl/meta wheel zoom and clamps the minimum scale", () => {
        const container = document.createElement("div");
        const onZoomChange = vi.fn();

        renderHook(() =>
            useMouseZoom({ current: container }, onZoomChange),
        );

        const zoomOutEvent = new WheelEvent("wheel", {
            deltaY: 1,
            ctrlKey: true,
            cancelable: true,
        });
        container.dispatchEvent(zoomOutEvent);

        expect(onZoomChange).toHaveBeenCalledTimes(1);
        expect(onZoomChange.mock.calls[0][0](0.15)).toBe(0.1);
        expect(zoomOutEvent.defaultPrevented).toBe(true);

        const zoomInEvent = new WheelEvent("wheel", {
            deltaY: -1,
            metaKey: true,
            cancelable: true,
        });
        container.dispatchEvent(zoomInEvent);

        expect(onZoomChange).toHaveBeenCalledTimes(2);
        expect(onZoomChange.mock.calls[1][0](1)).toBe(1.1);
    });

    it("ignores normal wheel scrolling and removes the listener on unmount", () => {
        const container = document.createElement("div");
        const onZoomChange = vi.fn();

        const { unmount } = renderHook(() =>
            useMouseZoom({ current: container }, onZoomChange),
        );

        container.dispatchEvent(new WheelEvent("wheel", { deltaY: -1 }));
        expect(onZoomChange).not.toHaveBeenCalled();

        unmount();
        container.dispatchEvent(
            new WheelEvent("wheel", { deltaY: -1, ctrlKey: true }),
        );

        expect(onZoomChange).not.toHaveBeenCalled();
    });
});
