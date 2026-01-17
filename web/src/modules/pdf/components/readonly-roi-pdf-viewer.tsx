import { useSvgZoom } from "@/modules/canvas/context/svg-zoom-context";
import { SvgCanvas } from "@/modules/canvas/svg-canvas";
import { PdfViewer } from "@/modules/pdf/components/pdf-viewer";
import { RoiSvg } from "@/modules/rois/components/roi-svg";
import type { RoiType } from "@/modules/rois/schema";
import { useSelectedRois } from "@/modules/template-editor/hooks/use-selected-rois";
import { useTemplateEditor } from "@/modules/template-editor/hooks/use-template-editor";
import { getScaleFromReferenceScale } from "@/modules/template-editor/utils/coordinates";
import type { TemplateType } from "@/modules/templates/schema";
import React, { useCallback, useRef } from "react";

export const ReadonlyRoiPdfViewer = ({
    fileId,
    template,
    onRoiClick,
    shouldRenderRoiFn: customShouldRenderRoiFn,
}: {
    fileId: string;
    template: TemplateType;
    onRoiClick?: (roiId: string) => void;
    shouldRenderRoiFn?: (roi: RoiType) => boolean;
}) => {
    const { scale, width } = useSvgZoom();

    const { rois } = useTemplateEditor();
    const { setSelectedRoiIds, isSelectedRoi } = useSelectedRois();

    const referenceScale = getScaleFromReferenceScale(
        width,
        scale,
        template.width
    );

    const containerRef = useRef<HTMLDivElement>(null);

    const handleRoiClick = useCallback(
        (e: React.MouseEvent, roiId: string) => {
            e.stopPropagation();

            setSelectedRoiIds([roiId]);

            onRoiClick?.(roiId);
        },
        [onRoiClick, setSelectedRoiIds]
    );

    const shouldRenderRoiFn = useCallback(
        (roi: RoiType) => {
            if (customShouldRenderRoiFn) {
                return customShouldRenderRoiFn(roi);
            }
            return true;
        },
        [customShouldRenderRoiFn]
    );

    const renderRoi = useCallback(
        (roi: RoiType) =>
            shouldRenderRoiFn(roi) || isSelectedRoi(roi.id) ? (
                <RoiSvg
                    key={roi.id}
                    roi={roi}
                    scale={referenceScale}
                    onRoiClick={handleRoiClick}
                    isSelected={isSelectedRoi(roi.id ?? "")}
                />
            ) : null,
        [shouldRenderRoiFn, referenceScale, handleRoiClick, isSelectedRoi]
    );

    return (
        <div className="w-full relative" ref={containerRef}>
            <PdfViewer fileId={fileId} />
            <SvgCanvas width={template.width} rois={rois} render={renderRoi} />
        </div>
    );
};
