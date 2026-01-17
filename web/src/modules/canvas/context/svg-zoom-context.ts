import { createContext, useContext } from "react";

type SvgZoomContextType = {
    scale: number;
    width: number;
};

export const SvgZoomContext = createContext<SvgZoomContextType | undefined>(
    undefined
);

export const useSvgZoom = () => {
    const context = useContext(SvgZoomContext);
    if (!context) {
        throw new Error("useSvgZoom must be used within a SvgZoomProvider");
    }
    return context;
};
