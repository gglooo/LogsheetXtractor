import { useSvgZoom } from "@/modules/canvas/context/svg-zoom-context";
import { SvgCanvas } from "@/modules/canvas/svg-canvas";
import { PdfDrawingSvgOverlay } from "@/modules/pdf/components/overlay/pdf-drawing-svg-overlay";
import { PdfViewer } from "@/modules/pdf/components/pdf-viewer";
import { useSplitTool } from "@/modules/pdf/hooks/use-split-tool";
import { getDuplicates, type Point } from "@/modules/pdf/utils";
import { RoiSvg } from "@/modules/rois/components/roi-svg";
import type { RoiType } from "@/modules/rois/schema";
import { useSelectToolEmptySelectionHelp } from "@/modules/template-editor/hooks/use-select-tool-empty-selection-help";
import { useSelectedRois } from "@/modules/template-editor/hooks/use-selected-rois";
import { useTemplateEditor } from "@/modules/template-editor/hooks/use-template-editor";
import { RoiValidationPresetContextMenu } from "@/modules/template-editor/sidebar/roi-validation/components/roi-validation-preset-context-menu";
import { useRoiValidationPresetContextMenu } from "@/modules/template-editor/sidebar/roi-validation/hooks/use-roi-validation-preset-context-menu";
import { getScaleFromReferenceScale } from "@/modules/template-editor/utils/coordinates";
import type { TemplateType } from "@/modules/templates/schema";
import React, { useCallback, useMemo, useRef } from "react";

export const DrawablePdfViewer = ({
    fileId,
    template,
}: {
    fileId: string;
    template: TemplateType;
}) => {
    const { scale, width } = useSvgZoom();

    const { rois, removeRoi, setRois, mode } = useTemplateEditor();
    const { setSelectedRoiIds, isSelectedRoi } = useSelectedRois();
    const { trackSelectionResult } = useSelectToolEmptySelectionHelp({
        mode,
    });
    const { handleOpenRoiContextMenu, menuProps } =
        useRoiValidationPresetContextMenu(template.isEditable);

    const duplicateRoiNames = useMemo(
        () => getDuplicates(rois.map((r) => r.variableName)),
        [rois]
    );

    const referenceScale = getScaleFromReferenceScale(
        width,
        scale,
        template.width
    );

    const containerRef = useRef<HTMLDivElement>(null);

    const getMouseCoordinatesRelativeToContainer = useCallback(
        (e: React.MouseEvent): Point | undefined => {
            if (containerRef.current === null) {
                return;
            }

            const rect = containerRef.current.getBoundingClientRect();

            return {
                x: (e.clientX - rect.left) / referenceScale,
                y: (e.clientY - rect.top) / referenceScale,
            };
        },
        [referenceScale]
    );

    const { handleSplit, setSplitRoiGuideLines, roiGuideLines } = useSplitTool(
        getMouseCoordinatesRelativeToContainer
    );

    const onRoiClick = useCallback(
        (e: React.MouseEvent, roiId: string) => {
            e.stopPropagation();
            if (mode === "split") {
                return handleSplit(e, roiId);
            }

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
        [handleSplit, mode, setSelectedRoiIds]
    );

    const onDragEnd = useCallback(
        (movedRois: RoiType[]) => {
            setRois(movedRois);
        },
        [setRois]
    );

    const onResizeEnd = useCallback(
        (resizedRois: RoiType[]) => {
            setRois(resizedRois);
        },
        [setRois]
    );

    const onDrawingFinish = useCallback(
        (affectedRoiCount: number) => {
            trackSelectionResult(affectedRoiCount);
        },
        [trackSelectionResult]
    );

    const renderRoi = useCallback(
        (
            roi: RoiType,
            onDragStart?: (e: React.MouseEvent<Element>, roiId: string) => void,
            onResizeStart?: (
                e: React.MouseEvent<Element>,
                roiId: string
            ) => void,
            isDragging?: boolean
        ) => (
            <RoiSvg
                key={roi.id}
                roi={roi}
                onDelete={removeRoi}
                scale={referenceScale}
                onRoiClick={onRoiClick}
                onRoiContextMenu={handleOpenRoiContextMenu}
                isSelected={isSelectedRoi(roi.id ?? "")}
                isDuplicate={duplicateRoiNames.has(roi.variableName)}
                onRoiDrag={onDragStart}
                onRoiResizeStart={onResizeStart}
                isResizeable={mode === "select" && isSelectedRoi(roi.id ?? "")}
                onMouseMove={setSplitRoiGuideLines}
                guideLineCoordinates={roiGuideLines}
                isDragging={isDragging}
            />
        ),
        [
            removeRoi,
            referenceScale,
            onRoiClick,
            handleOpenRoiContextMenu,
            isSelectedRoi,
            duplicateRoiNames,
            mode,
            setSplitRoiGuideLines,
            roiGuideLines,
        ]
    );

    return (
        <div className="w-full relative" ref={containerRef}>
            <PdfViewer fileId={fileId} />
            {mode === "draw" ? (
                <PdfDrawingSvgOverlay rois={rois} render={renderRoi} />
            ) : (
                <SvgCanvas
                    dragEnded={onDragEnd}
                    resizeEnded={onResizeEnd}
                    onFinishDrawing={onDrawingFinish}
                    width={template.width}
                    rois={rois}
                    render={renderRoi}
                />
            )}
            <RoiValidationPresetContextMenu {...menuProps} />
        </div>
    );
};
