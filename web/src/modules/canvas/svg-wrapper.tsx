import { Button } from "@/components/ui/button";
import { SvgZoomContext } from "@/modules/canvas/context/svg-zoom-context";
import { useMouseZoom } from "@/modules/pdf/hooks/use-mouse-zoom";
import { HistoryControls } from "@/modules/template-editor/components/history-controls";
import { RotateCcw, ZoomIn, ZoomOut } from "lucide-react";
import { useLayoutEffect, useRef, useState } from "react";

type SvgWrapperProps = {
    children: React.ReactNode;
    includeHistoryControls?: boolean;
    includeZoomControls?: boolean;
};

export const DEFAULT_SCALE = 0.6;

export const SvgWrapper = ({
    children,
    includeHistoryControls = true,
    includeZoomControls = true,
}: SvgWrapperProps) => {
    const [scale, setScale] = useState(DEFAULT_SCALE);
    const [width, setWidth] = useState(0);
    const containerRef = useRef<HTMLDivElement>(null);
    const scrollContainerRef = useRef<HTMLDivElement>(null);

    const handleZoomIn = () => setScale((prev) => prev + 0.2);
    const handleZoomOut = () => setScale((prev) => Math.max(prev - 0.2, 0.1));
    const handleReset = () => setScale(DEFAULT_SCALE);

    useLayoutEffect(() => {
        if (!scrollContainerRef.current) return;

        let timeoutId: ReturnType<typeof setTimeout>;

        const resizeObserver = new ResizeObserver((entries) => {
            for (const entry of entries) {
                clearTimeout(timeoutId);
                timeoutId = setTimeout(() => {
                    setWidth(entry.contentRect.width);
                }, 100);
            }
        });

        resizeObserver.observe(scrollContainerRef.current);

        return () => {
            resizeObserver.disconnect();
            clearTimeout(timeoutId);
        };
    }, []);

    useMouseZoom(scrollContainerRef, setScale);

    return (
        <div className="flex flex-col w-full h-full relative">
            <div className="flex flex-row justify-between px-8 z-10 bg-background/80 backdrop-blur-sm m-2 border-b">
                {includeHistoryControls ? <HistoryControls /> : null}
                {includeZoomControls ? (
                    <div className="flex gap-2 shrink-0 ">
                        <Button
                            variant="outline"
                            size="icon"
                            onClick={handleZoomOut}
                            disabled={scale <= 0.1}
                        >
                            <ZoomOut className="h-4 w-4" />
                        </Button>
                        <Button
                            variant="outline"
                            size="icon"
                            onClick={handleReset}
                            disabled={scale === DEFAULT_SCALE}
                        >
                            <RotateCcw className="h-4 w-4" />
                        </Button>
                        <Button
                            variant="outline"
                            size="icon"
                            onClick={handleZoomIn}
                        >
                            <ZoomIn className="h-4 w-4" />
                        </Button>
                    </div>
                ) : null}
            </div>

            <div
                className="flex-1 overflow-auto bg-muted/20"
                ref={scrollContainerRef}
            >
                <div
                    ref={containerRef}
                    style={{
                        width: width > 0 ? width * scale : "100%",
                        margin: "0 auto",
                        minHeight: "100%",
                        transformOrigin: "top left",
                    }}
                >
                    <SvgZoomContext.Provider value={{ scale, width }}>
                        {children}
                    </SvgZoomContext.Provider>
                </div>
            </div>
        </div>
    );
};
