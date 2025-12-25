import { Button } from "@/components/ui/button";
import { PdfZoomContext } from "@/modules/pdf/context/pdf-zoom-context";
import { HistoryControls } from "@/modules/template-editor/components/history-controls";
import { RotateCcw, ZoomIn, ZoomOut } from "lucide-react";
import { useLayoutEffect, useRef, useState } from "react";

type PdfWrapperProps = {
    children: React.ReactNode;
};

const DEFAULT_SCALE = 0.8;

export const PdfWrapper = ({ children }: PdfWrapperProps) => {
    const [scale, setScale] = useState(DEFAULT_SCALE);
    const [width, setWidth] = useState(0);
    const containerRef = useRef<HTMLDivElement>(null);

    const handleZoomIn = () => setScale((prev) => prev + 0.2);
    const handleZoomOut = () => setScale((prev) => Math.max(prev - 0.2, 0.1));
    const handleReset = () => setScale(DEFAULT_SCALE);

    useLayoutEffect(() => {
        if (!containerRef.current) return;

        let timeoutId: ReturnType<typeof setTimeout>;

        const resizeObserver = new ResizeObserver((entries) => {
            for (const entry of entries) {
                clearTimeout(timeoutId);
                timeoutId = setTimeout(() => {
                    setWidth(entry.contentRect.width);
                }, 100);
            }
        });

        resizeObserver.observe(containerRef.current);

        return () => {
            resizeObserver.disconnect();
            clearTimeout(timeoutId);
        };
    }, []);

    return (
        <div className="flex flex-col w-full">
            <div className="flex flex-row justify-between px-8">
                <HistoryControls />
                <div className="flex gap-2 mb-4 shrink-0 ">
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
            </div>

            <div
                className="flex-1 overflow-auto bg-muted/20"
                ref={containerRef}
            >
                <div
                    style={{
                        width: width > 0 ? width * scale : "100%",
                        margin: "0 auto",
                        minHeight: "100%",
                    }}
                >
                    <PdfZoomContext.Provider value={{ scale, width }}>
                        {children}
                    </PdfZoomContext.Provider>
                </div>
            </div>
        </div>
    );
};
