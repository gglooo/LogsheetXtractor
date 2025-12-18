import { useState } from "react";

export type Position = {
    x: number;
    y: number;
};

export const useDrawRectangle = (canDraw: boolean) => {
    const [startPos, setStartPos] = useState<Position | null>(null);
    const [currentPos, setCurrentPos] = useState<Position | null>(null);

    const handleStartDrawing = (e: React.MouseEvent<SVGSVGElement>) => {
        if (!canDraw) {
            return;
        }

        const rect = e.currentTarget.getBoundingClientRect();
        const x = e.clientX - rect.left;
        const y = e.clientY - rect.top;

        setStartPos({ x, y });
        setCurrentPos({ x, y });
    };

    const handleDraw = (e: React.MouseEvent<SVGSVGElement>) => {
        if (!startPos || !canDraw) {
            return;
        }

        const rect = e.currentTarget.getBoundingClientRect();
        const x = e.clientX - rect.left;
        const y = e.clientY - rect.top;

        setCurrentPos({ x, y });
    };

    const handleStopDrawing = (
        onFinishDrawing: (startPos: Position, currentPos: Position) => void
    ) => {
        if (!canDraw) {
            return;
        }

        if (startPos && currentPos) {
            onFinishDrawing(startPos, currentPos);
        }

        setStartPos(null);
        setCurrentPos(null);
    };

    return {
        startPos,
        currentPos,
        handleStartDrawing,
        handleDraw,
        handleStopDrawing,
    };
};
