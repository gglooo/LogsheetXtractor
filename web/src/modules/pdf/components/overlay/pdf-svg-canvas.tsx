import { PdfDrawingSvgOverlay } from "@/modules/pdf/components/overlay/pdf-drawing-svg-overlay";
import { PdfSelectSvgOverlay } from "@/modules/pdf/components/overlay/pdf-select-svg-overlay";
import { DragProvider } from "@/modules/pdf/context/drag-context";
import type { RoiType } from "@/modules/rois/schema";
import { useTemplateEditor } from "@/modules/template-editor/hooks/use-template-editor";

export type PdfCanvasRenderFn = (
    roi: RoiType,
    onDragStart?: (e: React.MouseEvent<Element>) => void,
    onResizeStart?: (e: React.MouseEvent<Element>, roiId: string) => void
) => React.ReactNode;

export const PdfSvgCanvas = ({
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
    const { mode } = useTemplateEditor();

    if (mode === "draw") {
        return <PdfDrawingSvgOverlay rois={rois} render={render} />;
    }

    return (
        <DragProvider>
            <PdfSelectSvgOverlay
                rois={rois}
                render={render}
                width={width}
                dragEnded={dragEnded}
                resizeEnded={resizeEnded}
            />
        </DragProvider>
    );
};
