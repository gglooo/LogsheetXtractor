import { getCoordinatesFromPositions } from "@/modules/template-editor/utils/coordinates";
import { useState } from "react";

export type Position = {
    x: number;
    y: number;
};

const MIN_SIZE = 5;

const isRoiReasonablySized = (coordinates: {
    x: number;
    y: number;
    width: number;
    height: number;
}) => {
    return coordinates.width >= MIN_SIZE && coordinates.height >= MIN_SIZE;
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

        if (
            startPos &&
            currentPos &&
            isRoiReasonablySized(
                getCoordinatesFromPositions(startPos, currentPos)
            )
        ) {
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
