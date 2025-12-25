/* eslint-disable react-hooks/refs */
import type { PdfCanvasRenderFn } from "@/modules/pdf/components/overlay/pdf-svg-canvas";
import { usePdfZoom } from "@/modules/pdf/context/pdf-zoom-context";
import { useDrag } from "@/modules/pdf/hooks/use-drag";
import {
    useDrawRectangle,
    type Position,
} from "@/modules/pdf/hooks/use-draw-rectangle";
import type { RoiType } from "@/modules/rois/schema";
import { useSelectedRois } from "@/modules/template-editor/hooks/use-selected-rois";
import {
    areCoordinatesOverlapping,
    getCoordinatesFromPositions,
    getScaleToReferenceScale,
    scaleCoordinatesToReference,
} from "@/modules/template-editor/utils/coordinates";
import type { Coordinates } from "@/schema";
import { useCallback, useLayoutEffect, useRef } from "react";

export const PdfSelectSvgOverlay = ({
    rois,
    render,
    width,
    dragEnded,
    resizeEnded,
}: {
    rois: RoiType[];
    render: PdfCanvasRenderFn;
    width: number;
    dragEnded?: (rois: RoiType[]) => void;
    resizeEnded?: (roi: RoiType) => void;
}) => {
    const { width: pdfWidth, scale } = usePdfZoom();
    const { isSelectedRoi, setSelectedRoiIds } = useSelectedRois();

    const dragControls = useDrag();
    const dragControlsRef = useRef(dragControls);

    useLayoutEffect(() => {
        dragControlsRef.current = dragControls;
    });

    const {
        handleStartDrawing,
        handleDraw,
        handleStopDrawing,
        startPos: drawStartPos,
        currentPos: drawCurrentPos,
    } = useDrawRectangle(true);

    const getMovedRois = () =>
        rois.map((roi) => {
            if (
                !isSelectedRoi(roi.id) ||
                (!dragControls.isDragging && !dragControls.isResizing)
            ) {
                return roi;
            }

            const toReferenceScale = getScaleToReferenceScale(
                pdfWidth,
                scale,
                width
            );

            const coordinates: Coordinates = {
                x: roi.coordinates.x + dragControls.dx * toReferenceScale,
                y: roi.coordinates.y + dragControls.dy * toReferenceScale,
                height:
                    roi.coordinates.height + dragControls.dh * toReferenceScale,
                width:
                    roi.coordinates.width + dragControls.dw * toReferenceScale,
            };

            return {
                ...roi,
                coordinates: {
                    x:
                        coordinates.width < 0
                            ? coordinates.x + coordinates.width
                            : coordinates.x,
                    y:
                        coordinates.height < 0
                            ? coordinates.y + coordinates.height
                            : coordinates.y,
                    width: Math.abs(coordinates.width),
                    height: Math.abs(coordinates.height),
                },
            };
        });

    const onDragEnd = () => {
        dragEnded?.(getMovedRois());
        dragControls.handleDragEnd();
    };

    const onDragStart = useCallback(
        (e: React.MouseEvent<Element>, roiId: string) => {
            if (!isSelectedRoi(roiId)) {
                return;
            }
            dragControlsRef.current.handleDragStart(e);
        },
        [isSelectedRoi]
    );

    const onResizeStart = useCallback(
        (e: React.MouseEvent<Element>, roiId: string) => {
            resizeEnded?.(rois.find((roi) => roi.id === roiId)!);
            dragControlsRef.current.handleResizeStart(e);
        },
        [rois, resizeEnded]
    );

    const onResizeEnd = () => {
        resizeEnded?.(getMovedRois().find((roi) => isSelectedRoi(roi.id))!);
        dragControls.handleResizeEnd();
    };

    const onFinishDrawing = (startPos: Position, currentPos: Position) => {
        const coordinates = getCoordinatesFromPositions(startPos, currentPos);
        const toReferenceScale = getScaleToReferenceScale(
            pdfWidth,
            scale,
            width
        );
        const scaledCoordinates = scaleCoordinatesToReference(
            coordinates,
            toReferenceScale
        );

        const selectedRois = rois.filter((roi) =>
            areCoordinatesOverlapping(roi.coordinates, scaledCoordinates)
        );

        setSelectedRoiIds(selectedRois.map((roi) => roi.id!));
    };

    const onMouseMove = dragControls.isDragging
        ? dragControls.handleDrag
        : dragControls.isResizing
        ? dragControls.handleResize
        : handleDraw;

    const onMouseUp = dragControls.isDragging
        ? onDragEnd
        : dragControls.isResizing
        ? onResizeEnd
        : () => handleStopDrawing(onFinishDrawing);

    return (
        <svg
            className="absolute top-0 left-0 w-full h-full pointer-events-auto"
            onMouseDown={
                !dragControls.isDragging && !dragControls.isResizing
                    ? handleStartDrawing
                    : undefined
            }
            onMouseMove={onMouseMove}
            onMouseUp={onMouseUp}
            onClick={() => setSelectedRoiIds([])}
            style={{ zIndex: 20 }}
        >
            {getMovedRois().map((roi) => {
                return render(roi, onDragStart, onResizeStart);
            })}
            {drawStartPos && drawCurrentPos
                ? (() => {
                      const coordinates = getCoordinatesFromPositions(
                          drawStartPos,
                          drawCurrentPos
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
