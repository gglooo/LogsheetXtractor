import { usePdfZoom } from "@/modules/pdf/context/pdf-zoom-context";
import {
    useDrawRectangle,
    type Position,
} from "@/modules/pdf/hooks/use-draw-rectangle";
import { useTemplateEditor } from "@/modules/template-editor/hooks/use-template-editor";
import {
    getScaleToReferenceScale,
    scaleCoordinatesToReference,
} from "@/modules/template-editor/utils/coordinates";
import type { Coordinates } from "@/schema";
import type { PropsWithChildren } from "react";

const getCoordinatesFromPositions = (
    startPos: Position,
    currentPos: Position
): Coordinates => {
    const x = Math.min(startPos.x, currentPos.x);
    const y = Math.min(startPos.y, currentPos.y);
    const width = Math.abs(startPos.x - currentPos.x);
    const height = Math.abs(startPos.y - currentPos.y);

    return { x, y, width, height };
};

export const PdfSvgOverlay = ({ children }: PropsWithChildren) => {
    const { addRoi, template, mode } = useTemplateEditor();
    const { width: pdfWidth, scale } = usePdfZoom();

    const canDraw = mode === "draw";

    const {
        handleStartDrawing,
        handleDraw,
        handleStopDrawing,
        startPos,
        currentPos,
    } = useDrawRectangle(canDraw);

    const handleCreateRoi = (startPos: Position, currentPos: Position) => {
        const coordinates = getCoordinatesFromPositions(startPos, currentPos);

        const referenceScale = getScaleToReferenceScale(
            pdfWidth,
            scale,
            template!.width
        );

        addRoi(scaleCoordinatesToReference(coordinates, referenceScale));
    };

    return (
        <svg
            className="absolute top-0 left-0 w-full h-full pointer-events-auto"
            onMouseDown={(e) => handleStartDrawing(e)}
            onMouseMove={(e) => handleDraw(e)}
            onMouseUp={() => handleStopDrawing(handleCreateRoi)}
            style={{ zIndex: 50 }}
        >
            {children}
            {startPos && currentPos
                ? (() => {
                      const coordinates = getCoordinatesFromPositions(
                          startPos,
                          currentPos
                      );
                      return (
                          <rect
                              x={coordinates.x}
                              y={coordinates.y}
                              width={coordinates.width}
                              height={coordinates.height}
                              fill="rgba(0, 123, 255, 0.3)"
                              stroke="rgba(0, 123, 255, 0.8)"
                              strokeWidth={2}
                          />
                      );
                  })()
                : null}
        </svg>
    );
};
