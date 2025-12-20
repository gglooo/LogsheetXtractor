import { PdfSvgOverlay } from "@/modules/pdf/components/pdf-svg-overlay";
import { PdfViewer } from "@/modules/pdf/components/pdf-viewer";
import { usePdfZoom } from "@/modules/pdf/context/pdf-zoom-context";
import { RoiSvg } from "@/modules/rois/components/roi-svg";
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

    const { rois, removeRoi } = useTemplateEditor();
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

    return (
        <div className="w-full relative">
            <PdfViewer fileId={fileId} />
            <PdfSvgOverlay>
                {rois.map((roi) => (
                    <RoiSvg
                        key={roi.id}
                        roi={roi}
                        onDelete={removeRoi}
                        scale={referenceScale}
                        onRoiClick={onRoiClick}
                        isSelected={isSelectedRoi(roi.id ?? "")}
                    />
                ))}
            </PdfSvgOverlay>
        </div>
    );
};
