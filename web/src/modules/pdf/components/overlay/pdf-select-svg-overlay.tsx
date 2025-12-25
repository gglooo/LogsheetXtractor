import type { PdfCanvasRenderFn } from "@/modules/pdf/components/overlay/pdf-svg-canvas";
import { usePdfZoom } from "@/modules/pdf/context/pdf-zoom-context";
import { useDrag } from "@/modules/pdf/hooks/use-drag";
import type { RoiType } from "@/modules/rois/schema";
import { useSelectedRois } from "@/modules/template-editor/hooks/use-selected-rois";
import { getScaleToReferenceScale } from "@/modules/template-editor/utils/coordinates";
import type { Coordinates } from "@/schema";

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

    const {
        handleDragStart,
        handleDrag,
        handleDragEnd,
        dx,
        dy,
        dw,
        dh,
        isDragging,
        isResizing,
        handleResizeStart,
        handleResize,
        handleResizeEnd,
    } = useDrag();

    const getMovedRois = () =>
        rois.map((roi) => {
            if (!isSelectedRoi(roi.id) || (!isDragging && !isResizing)) {
                return roi;
            }

            const toReferenceScale = getScaleToReferenceScale(
                pdfWidth,
                scale,
                width
            );

            const coordinates: Coordinates = {
                x: roi.coordinates.x + dx * toReferenceScale,
                y: roi.coordinates.y + dy * toReferenceScale,
                height: roi.coordinates.height + dh * toReferenceScale,
                width: roi.coordinates.width + dw * toReferenceScale,
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
        handleDragEnd();
    };

    const onDragStart = (e: React.MouseEvent<Element>, roiId: string) => {
        if (!isSelectedRoi(roiId)) {
            return;
        }
        handleDragStart(e);
    };

    const onResizeStart = (e: React.MouseEvent<Element>, roiId: string) => {
        resizeEnded?.(rois.find((roi) => roi.id === roiId)!);
        handleResizeStart(e);
    };

    const onResizeEnd = () => {
        resizeEnded?.(getMovedRois().find((roi) => isSelectedRoi(roi.id))!);
        handleResizeEnd();
    };

    return (
        <svg
            className="absolute top-0 left-0 w-full h-full pointer-events-auto"
            onMouseMove={isDragging ? handleDrag : handleResize}
            onMouseUp={isDragging ? onDragEnd : onResizeEnd}
            onClick={() => setSelectedRoiIds([])}
            style={{ zIndex: 20 }}
        >
            {getMovedRois().map((roi) => {
                return render(
                    roi,
                    (e) => onDragStart(e, roi.id!),
                    (e) => onResizeStart(e, roi.id!)
                );
            })}
        </svg>
    );
};
