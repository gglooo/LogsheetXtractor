/* eslint-disable react-hooks/refs */
import { useSvgZoom } from "@/modules/canvas/context/svg-zoom-context";
import type { SvgCanvasRenderFn } from "@/modules/canvas/svg-canvas";
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

type InteractionMode = "drawing" | "dragging" | "resizing" | null;

export const SelectSvgOverlay = ({
    rois,
    render,
    width,
    dragEnded,
    resizeEnded,
}: {
    rois: RoiType[];
    render: SvgCanvasRenderFn;
    width: number;
    dragEnded?: (rois: RoiType[]) => void;
    resizeEnded?: (rois: RoiType[]) => void;
}) => {
    const { width: pdfWidth, scale } = useSvgZoom();
    const { isSelectedRoi, setSelectedRoiIds } = useSelectedRois();

    const interactionMode = useRef<InteractionMode>(null);
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
                width,
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

    const onDragStart = useCallback(
        (e: React.MouseEvent<Element>, roiId: string) => {
            if (!isSelectedRoi(roiId)) {
                return;
            }
            interactionMode.current = "dragging";
            dragControlsRef.current.handleDragStart(e);
        },
        [isSelectedRoi],
    );

    const onResizeStart = useCallback((e: React.MouseEvent<Element>) => {
        interactionMode.current = "resizing";
        dragControlsRef.current.handleResizeStart(e);
    }, []);

    const onBgMouseDown = (e: React.MouseEvent) => {
        if (e.button !== 0) {
            return;
        }
        interactionMode.current = "drawing";
        handleStartDrawing(e);
    };

    const onUnifiedMouseMove = (e: React.MouseEvent) => {
        if (interactionMode.current === "dragging") {
            dragControls.handleDrag(e);
        } else if (interactionMode.current === "resizing") {
            dragControls.handleResize(e);
        } else if (interactionMode.current === "drawing") {
            handleDraw(e);
        }
    };

    const onUnifiedMouseUp = (e: React.MouseEvent) => {
        if (interactionMode.current === "dragging") {
            e.stopPropagation();
            if (dragControls.isDragging) {
                dragEnded?.(getMovedRois());
            }
            dragControls.handleDragEnd();
        } else if (interactionMode.current === "resizing") {
            e.stopPropagation();
            resizeEnded?.(getMovedRois());
            dragControls.handleResizeEnd();
        } else if (interactionMode.current === "drawing") {
            handleStopDrawing(onFinishDrawing);
        }

        interactionMode.current = null;
    };

    const onFinishDrawing = (startPos: Position, currentPos: Position) => {
        const coordinates = getCoordinatesFromPositions(startPos, currentPos);
        const toReferenceScale = getScaleToReferenceScale(
            pdfWidth,
            scale,
            width,
        );
        const scaledCoordinates = scaleCoordinatesToReference(
            coordinates,
            toReferenceScale,
        );

        const selectedRois = rois.filter((roi) =>
            areCoordinatesOverlapping(roi.coordinates, scaledCoordinates),
        );

        setSelectedRoiIds(selectedRois.map((roi) => roi.id!));
    };

    return (
        <svg
            className="absolute top-0 left-0 w-full h-full pointer-events-auto"
            onMouseDown={onBgMouseDown}
            onMouseMove={onUnifiedMouseMove}
            onMouseUp={onUnifiedMouseUp}
            onMouseLeave={onUnifiedMouseUp}
            onClick={() => {
                if (dragControls.isDragging || dragControls.isResizing) {
                    return;
                }
                setSelectedRoiIds([]);
            }}
            style={{ zIndex: 10 }}
        >
            {getMovedRois().map((roi) => {
                return render(
                    roi,
                    onDragStart,
                    onResizeStart,
                    dragControls.isDragging || dragControls.isResizing,
                );
            })}
            {drawStartPos && drawCurrentPos
                ? (() => {
                      const coordinates = getCoordinatesFromPositions(
                          drawStartPos,
                          drawCurrentPos,
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
