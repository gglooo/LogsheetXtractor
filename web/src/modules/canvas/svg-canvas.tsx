import { SelectSvgOverlay } from "@/modules/pdf/components/overlay/pdf-select-svg-overlay";
import { DragProvider } from "@/modules/pdf/context/drag-context";
import type { RoiType } from "@/modules/rois/schema";

export type SvgCanvasRenderFn = (
    roi: RoiType,
    onDragStart?: (e: React.MouseEvent<Element>, roiId: string) => void,
    onResizeStart?: (e: React.MouseEvent<Element>, roiId: string) => void,
    isDragging?: boolean
) => React.ReactNode;

export const SvgCanvas = ({
    rois,
    render,
    width,
    dragEnded,
    resizeEnded,
    onFinishDrawing,
}: {
    rois: RoiType[];
    render: SvgCanvasRenderFn;
    width: number;
    dragEnded?: (rois: RoiType[]) => void;
    resizeEnded?: (rois: RoiType[]) => void;
    onFinishDrawing?: (affectedRoiCount: number) => void;
}) => {
    return (
        <DragProvider>
            <SelectSvgOverlay
                rois={rois}
                render={render}
                width={width}
                dragEnded={dragEnded}
                resizeEnded={resizeEnded}
                onFinishDrawing={onFinishDrawing}
            />
        </DragProvider>
    );
};
