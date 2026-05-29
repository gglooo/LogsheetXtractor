import {
    splitRoiHorizontally,
    splitRoiVertically,
    type Point,
} from "@/modules/pdf/utils";
import type { RoiType } from "@/modules/rois/schema";
import { useTemplateEditor } from "@/modules/template-editor/hooks/use-template-editor";
import { areCoordinatesOverlapping } from "@/modules/template-editor/utils/coordinates";
import type { Coordinates } from "@/schema";
import { useCallback, useState } from "react";

export const useSplitTool = (
    getRelativeCoordinates: (e: React.MouseEvent) => Point | undefined
) => {
    const { rois, getNewRoi, setRois, mode, setMode } = useTemplateEditor();

    const isSplitVertical = (e: React.MouseEvent, roi: RoiType) => {
        const isCtrl = e.metaKey || e.ctrlKey;
        const isVertical = roi.coordinates.width >= roi.coordinates.height;

        return isCtrl ? !isVertical : isVertical;
    };

    const handleSplit = useCallback(
        (e: React.MouseEvent, roiId: string) => {
            const roi = rois.find((r) => r.id === roiId);
            if (!roi) {
                return;
            }

            const splitPoint = getRelativeCoordinates(e);
            if (!splitPoint) {
                return;
            }

            const [firstCoordinates, secondCoordinates] = isSplitVertical(
                e,
                roi
            )
                ? splitRoiVertically(roi, splitPoint)
                : splitRoiHorizontally(roi, splitPoint);

            setRois((prevRois) => [
                ...prevRois.filter((r) => r.id !== roiId),
                getNewRoi(firstCoordinates),
                getNewRoi(secondCoordinates),
            ]);
            setMode("select");
        },
        [getNewRoi, rois, setRois, setMode, getRelativeCoordinates]
    );

    const [roiGuideLines, setRoiGuideLines] = useState<Coordinates>();
    const setSplitRoiGuideLines = useCallback(
        (e: React.MouseEvent, roi?: RoiType) => {
            if (mode !== "split" || !roi) {
                setRoiGuideLines(undefined);
                return;
            }

            const mousePoint = getRelativeCoordinates(e);
            if (!mousePoint) {
                return;
            }

            const shouldDrawLine = areCoordinatesOverlapping(roi.coordinates, {
                ...mousePoint,
                width: 1,
                height: 1,
            });

            if (!shouldDrawLine) {
                setRoiGuideLines(undefined);
                return;
            }

            if (isSplitVertical(e, roi)) {
                setRoiGuideLines({
                    x: mousePoint.x,
                    y: roi.coordinates.y,
                    width: 3,
                    height: roi.coordinates.height,
                });
            } else {
                setRoiGuideLines({
                    x: roi.coordinates.x,
                    y: mousePoint.y,
                    width: roi.coordinates.width,
                    height: 3,
                });
            }
        },
        [getRelativeCoordinates, mode]
    );

    return {
        handleSplit,
        setSplitRoiGuideLines,
        roiGuideLines,
    };
};
