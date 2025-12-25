import { PdfSvgCanvas } from "@/modules/pdf/components/overlay/pdf-svg-canvas";
import { PdfViewer } from "@/modules/pdf/components/pdf-viewer";
import { usePdfZoom } from "@/modules/pdf/context/pdf-zoom-context";
import { RoiSvg } from "@/modules/rois/components/roi-svg";
import type { RoiType } from "@/modules/rois/schema";
import { useSelectedRois } from "@/modules/template-editor/hooks/use-selected-rois";
import { useTemplateEditor } from "@/modules/template-editor/hooks/use-template-editor";
import { getScaleFromReferenceScale } from "@/modules/template-editor/utils/coordinates";
import type { TemplateType } from "@/modules/templates/schema";

export const DrawablePdfViewer = ({
    fileId,
    template,
}: {
    fileId: string;
    template: TemplateType;
}) => {
    const { scale, width } = usePdfZoom();

    const { rois, removeRoi, setRois, mode } = useTemplateEditor();
    const { setSelectedRoiIds, isSelectedRoi } = useSelectedRois();

    const referenceScale = getScaleFromReferenceScale(
        width,
        scale,
        template.width
    );

    const onRoiClick = (e: React.MouseEvent, roiId: string) => {
        e.stopPropagation();
        if (e.ctrlKey || e.metaKey) {
            if (isSelectedRoi(roiId)) {
                setSelectedRoiIds((prev) => prev.filter((id) => id !== roiId));
            } else {
                setSelectedRoiIds((prev) => [...prev, roiId]);
            }
            return;
        }

        setSelectedRoiIds([roiId]);
    };

    const onDragEnd = (movedRois: RoiType[]) => {
        movedRois.forEach((element) => {
            console.log("moved roi", element.coordinates);
        });
        setRois(movedRois);
    };

    const onResizeEnd = (resizedRoi: RoiType) => {
        setRois(
            rois.map((roi) => (roi.id === resizedRoi.id ? resizedRoi : roi))
        );
    };

    return (
        <div className="w-full relative">
            <PdfViewer fileId={fileId} />
            <PdfSvgCanvas
                dragEnded={onDragEnd}
                resizeEnded={onResizeEnd}
                width={template.width}
                rois={rois}
                render={(roi, onDragStart, onResizeStart) => (
                    <RoiSvg
                        key={roi.id}
                        roi={roi}
                        onDelete={removeRoi}
                        scale={referenceScale}
                        onRoiClick={onRoiClick}
                        isSelected={isSelectedRoi(roi.id ?? "")}
                        onRoiDrag={onDragStart}
                        onRoiResizeStart={onResizeStart}
                        isResizeable={mode === "select"}
                    />
                )}
            />
        </div>
    );
};
