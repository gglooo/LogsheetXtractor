import { DragContext } from "@/modules/pdf/hooks/use-drag";
import { useState, type PropsWithChildren } from "react";

const PIXEL_DRAG_THRESHOLD = 5;

export const DragProvider = ({ children }: PropsWithChildren) => {
    const [startX, setStartX] = useState(0);
    const [startY, setStartY] = useState(0);
    const [startW, setStartW] = useState(0);
    const [startH, setStartH] = useState(0);
    const [currentW, setCurrentW] = useState(0);
    const [currentH, setCurrentH] = useState(0);
    const [currentX, setCurrentX] = useState(0);
    const [currentY, setCurrentY] = useState(0);

    const [isMouseDown, setIsMouseDown] = useState(false);
    const [isDragging, setIsDragging] = useState(false);
    const [isResizing, setIsResizing] = useState(false);

    const handleResizeStart = (e: React.MouseEvent<Element>) => {
        if (isDragging || isResizing) {
            return;
        }

        setIsMouseDown(true);
        setStartW(e.clientX);
        setStartH(e.clientY);
        setCurrentW(e.clientX);
        setCurrentH(e.clientY);
    };

    const handleDragStart = (e: React.MouseEvent<Element>) => {
        if (isDragging || isResizing) {
            return;
        }

        setIsMouseDown(true);
        setStartX(e.clientX);
        setStartY(e.clientY);
        setCurrentX(e.clientX);
        setCurrentY(e.clientY);
    };

    const handleDrag = (e: React.MouseEvent<Element>) => {
        if (!isMouseDown) {
            return;
        }

        if (!isDragging) {
            const dx = Math.abs(e.clientX - startX);
            const dy = Math.abs(e.clientY - startY);

            if (dx + dy > PIXEL_DRAG_THRESHOLD) {
                setIsDragging(true);
            }
        }

        if (!isDragging) {
            return;
        }

        setCurrentX(e.clientX);
        setCurrentY(e.clientY);
    };

    const handleResize = (e: React.MouseEvent<Element>) => {
        if (!isMouseDown) {
            return;
        }

        if (!isResizing) {
            const dx = Math.abs(e.clientX - startW);
            const dy = Math.abs(e.clientY - startH);

            if (dx + dy > PIXEL_DRAG_THRESHOLD) {
                setIsResizing(true);
            }
        }

        if (!isResizing) {
            return;
        }

        setCurrentW(e.clientX);
        setCurrentH(e.clientY);
    };

    const handleDragEnd = () => {
        setStartX(0);
        setStartY(0);
        setCurrentX(0);
        setCurrentY(0);

        setIsMouseDown(false);

        // Delay so that any onClick events don't get swallowed
        setTimeout(() => {
            setIsDragging(false);
        }, 0);
    };

    const handleResizeEnd = () => {
        setStartW(0);
        setStartH(0);
        setCurrentW(0);
        setCurrentH(0);

        setIsMouseDown(false);

        setTimeout(() => {
            setIsResizing(false);
        }, 0);
    };

    return (
        <DragContext.Provider
            value={{
                isDragging,
                dx: currentX - startX,
                dy: currentY - startY,
                isResizing,
                dw: currentW - startW,
                dh: currentH - startH,
                handleResizeStart,
                handleResize,
                handleResizeEnd,
                handleDragStart,
                handleDrag,
                handleDragEnd,
            }}
        >
            {children}
        </DragContext.Provider>
    );
};
