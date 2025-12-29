import { PdfSvgCanvas } from "@/modules/pdf/components/overlay/pdf-svg-canvas";
import { PdfViewer } from "@/modules/pdf/components/pdf-viewer";
import { usePdfZoom } from "@/modules/pdf/context/pdf-zoom-context";
import { getDuplicates } from "@/modules/pdf/utils";
import { RoiSvg } from "@/modules/rois/components/roi-svg";
import type { RoiType } from "@/modules/rois/schema";
import { useSelectedRois } from "@/modules/template-editor/hooks/use-selected-rois";
import { useTemplateEditor } from "@/modules/template-editor/hooks/use-template-editor";
import { getScaleFromReferenceScale } from "@/modules/template-editor/utils/coordinates";
import type { TemplateType } from "@/modules/templates/schema";
import { useCallback, useMemo } from "react";

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

    const duplicateRoiNames = useMemo(
        () => getDuplicates(rois.map((r) => r.variableName)),
        [rois]
    );

    const referenceScale = getScaleFromReferenceScale(
        width,
        scale,
        template.width
    );

    const onRoiClick = useCallback(
        (e: React.MouseEvent, roiId: string) => {
            e.stopPropagation();
            if (e.ctrlKey || e.metaKey) {
                setSelectedRoiIds((prev) => {
                    if (prev.includes(roiId)) {
                        return prev.filter((id) => id !== roiId);
                    }
                    return [...prev, roiId];
                });
                return;
            }

            setSelectedRoiIds([roiId]);
        },
        [setSelectedRoiIds]
    );

    const onDragEnd = useCallback(
        (movedRois: RoiType[]) => {
            setRois(movedRois);
        },
        [setRois]
    );

    const onResizeEnd = useCallback(
        (resizedRoi: RoiType) => {
            setRois((prevRois) =>
                prevRois.map((roi) =>
                    roi.id === resizedRoi.id ? resizedRoi : roi
                )
            );
        },
        [setRois]
    );

    const renderRoi = useCallback(
        (
            roi: RoiType,
            onDragStart?: (e: React.MouseEvent<Element>, roiId: string) => void,
            onResizeStart?: (
                e: React.MouseEvent<Element>,
                roiId: string
            ) => void
        ) => (
            <RoiSvg
                key={roi.id}
                roi={roi}
                onDelete={removeRoi}
                scale={referenceScale}
                onRoiClick={onRoiClick}
                isSelected={isSelectedRoi(roi.id ?? "")}
                isDuplicate={duplicateRoiNames.has(roi.variableName)}
                onRoiDrag={onDragStart}
                onRoiResizeStart={onResizeStart}
                isResizeable={mode === "select"}
            />
        ),
        [
            removeRoi,
            referenceScale,
            onRoiClick,
            isSelectedRoi,
            duplicateRoiNames,
            mode,
        ]
    );

    return (
        <div className="w-full relative">
            <PdfViewer fileId={fileId} />
            <PdfSvgCanvas
                dragEnded={onDragEnd}
                resizeEnded={onResizeEnd}
                width={template.width}
                rois={rois}
                render={renderRoi}
            />
        </div>
    );
};
