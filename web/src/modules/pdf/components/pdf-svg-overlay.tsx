import { usePdfZoom } from "@/modules/pdf/context/pdf-zoom-context";
import {
    useDrawRectangle,
    type Position,
} from "@/modules/pdf/hooks/use-draw-rectangle";
import { useSelectedRois } from "@/modules/template-editor/hooks/use-selected-rois";
import { useTemplateEditor } from "@/modules/template-editor/hooks/use-template-editor";
import {
    getCoordinatesFromPositions,
    getScaleToReferenceScale,
    scaleCoordinatesToReference,
} from "@/modules/template-editor/utils/coordinates";
import type { PropsWithChildren } from "react";

export const PdfSvgOverlay = ({ children }: PropsWithChildren) => {
    const { addRoi, template, mode } = useTemplateEditor();
    const { width: pdfWidth, scale } = usePdfZoom();
    const { setSelectedRoiIds } = useSelectedRois();

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

        const newId = addRoi(
            scaleCoordinatesToReference(coordinates, referenceScale)
        );
        if (newId) {
            setSelectedRoiIds([newId]);
        }
    };

    return (
        <svg
            className="absolute top-0 left-0 w-full h-full pointer-events-auto"
            onMouseDown={(e) => handleStartDrawing(e)}
            onMouseMove={(e) => handleDraw(e)}
            onMouseUp={() => handleStopDrawing(handleCreateRoi)}
            style={{ zIndex: 20 }}
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
