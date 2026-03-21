import { createContext, useContext } from "react";

type DragContextType = {
    isDragging: boolean;
    dx: number;
    dy: number;
    dw: number;
    dh: number;
    isResizing: boolean;
    handleResizeStart: (e: React.MouseEvent<Element>) => void;
    handleResize: (e: React.MouseEvent<Element>) => void;
    handleResizeEnd: () => void;
    handleDragStart: (e: React.MouseEvent<Element>) => void;
    handleDrag: (e: React.MouseEvent<Element>) => void;
    handleDragEnd: () => void;
};

export const DragContext = createContext<DragContextType | undefined>(
    undefined
);

export const useDrag = () => {
    const context = useContext(DragContext);
    if (!context) {
        throw new Error("useDrag must be used within a DragProvider");
    }
    return context;
};
