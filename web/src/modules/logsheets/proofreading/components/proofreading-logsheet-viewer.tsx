import { Spinner } from "@/components/ui/spinner";
import { getUrlFromBytes } from "@/lib/utils";
import { useSvgZoom } from "@/modules/canvas/context/svg-zoom-context";
import { SvgCanvas } from "@/modules/canvas/svg-canvas";
import { useLogsheetImage } from "@/modules/logsheets/api";
import type { LogsheetType } from "@/modules/logsheets/schema";
import { RoiSvg } from "@/modules/rois/components/roi-svg";
import type { RoiType } from "@/modules/rois/schema";
import { useSelectedRois } from "@/modules/template-editor/hooks/use-selected-rois";
import { useTemplateEditor } from "@/modules/template-editor/hooks/use-template-editor";
import { getScaleFromReferenceScale } from "@/modules/template-editor/utils/coordinates";
import type { TemplateType } from "@/modules/templates/schema";
import React, { useCallback, useRef } from "react";

export const ProofreadingLogsheetViewer = ({
    logsheet,
    template,
    onRoiClick,
    shouldRenderRoiFn: customShouldRenderRoiFn,
}: {
    logsheet: LogsheetType;
    template: TemplateType;
    onRoiClick?: (roiId: string) => void;
    shouldRenderRoiFn?: (roi: RoiType) => boolean;
}) => {
    const { scale, width } = useSvgZoom();

    const { setSelectedRoiIds, isSelectedRoi } = useSelectedRois();

    const { rois } = useTemplateEditor();

    const logsheetImageQuery = useLogsheetImage(logsheet.id);

    const referenceScale = getScaleFromReferenceScale(
        width,
        scale,
        template.width,
    );

    const containerRef = useRef<HTMLDivElement>(null);

    const handleRoiClick = useCallback(
        (e: React.MouseEvent, roiId: string) => {
            e.stopPropagation();

            setSelectedRoiIds([roiId]);

            onRoiClick?.(roiId);
        },
        [onRoiClick, setSelectedRoiIds],
    );

    const shouldRenderRoiFn = useCallback(
        (roi: RoiType) => {
            if (customShouldRenderRoiFn) {
                return customShouldRenderRoiFn(roi);
            }
            return true;
        },
        [customShouldRenderRoiFn],
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
        [shouldRenderRoiFn, referenceScale, handleRoiClick, isSelectedRoi],
    );

    return (
        <div className="w-full relative" ref={containerRef}>
            {logsheetImageQuery.isLoading ? (
                <div className="w-full h-full flex items-center justify-center">
                    <Spinner />
                </div>
            ) : logsheetImageQuery.isError ? (
                <p className="text-red-500">Error loading image</p>
            ) : (
                <img
                    src={getUrlFromBytes(logsheetImageQuery.data!.bytes)}
                    alt="Logsheet"
                />
            )}
            <SvgCanvas
                width={template.width}
                rois={rois ?? []}
                render={renderRoi}
            />
        </div>
    );
};
