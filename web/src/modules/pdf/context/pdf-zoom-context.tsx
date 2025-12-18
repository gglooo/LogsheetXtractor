import { createContext, useContext } from "react";

interface PdfZoomContextType {
    scale: number;
    width: number;
}

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
