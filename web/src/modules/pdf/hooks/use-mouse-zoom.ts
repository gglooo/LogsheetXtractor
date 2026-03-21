import { useEffect } from "react";

export const useMouseZoom = (
    scrollContainerRef: React.RefObject<HTMLDivElement | null>,
    onZoomChange: (zoomSetter: (prev: number) => number) => void
) => {
    useEffect(() => {
        const container = scrollContainerRef.current;
        if (!container) return;

        const onWheel = (e: WheelEvent) => {
            if (e.ctrlKey || e.metaKey) {
                e.preventDefault();
                const delta = e.deltaY > 0 ? -0.1 : 0.1;
                onZoomChange((prev) => Math.max(0.1, prev + delta));
            }
        };

        container.addEventListener("wheel", onWheel, { passive: false });
        return () => container.removeEventListener("wheel", onWheel);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);
};
