import React, { createContext, useContext } from "react";

type PdfZoomContextType = {
    scale: number;
    width: number;
};

export const PdfZoomContext = createContext<PdfZoomContextType | undefined>(
    undefined
);

export const usePdfZoom = () => {
    const context = useContext(PdfZoomContext);
    if (!context) {
        throw new Error("usePdfZoom must be used within a PdfZoomProvider");
    }
    return context;
};

export const PdfZoomProvider = ({
    children,
}: {
    children: React.ReactNode;
}) => {
    const [scale] = React.useState(1);
    const [width, setWidth] = React.useState(0);

    // This is a simplified provider for now as we don't have the full zoom logic
    // We would need to implement useResizeObserver etc.
    // For now assuming full width
    const containerRef = React.useRef<HTMLDivElement>(null);

    React.useLayoutEffect(() => {
        if (containerRef.current) {
            setWidth(containerRef.current.offsetWidth);
        }
    }, []);

    return (
        <PdfZoomContext.Provider value={{ scale, width }}>
            <div className="w-full h-full" ref={containerRef}>
                {children}
            </div>
        </PdfZoomContext.Provider>
    );
};
