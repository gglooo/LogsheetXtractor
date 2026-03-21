import { useSelectedRois } from "@/modules/template-editor/hooks/use-selected-rois";
import { useTemplateEditor } from "@/modules/template-editor/hooks/use-template-editor";
import { useCallback, useState } from "react";

const getNextIndex = (currentIndex: number, length: number) => {
    return (currentIndex + 1) % length;
};

export const useBrowseSelectedRois = () => {
    const { rois } = useTemplateEditor();

    const [currentIndex, setCurrentIndex] = useState(rois.length);
    const { selectedRoiIds, setSelectedRoiIds } = useSelectedRois();

    const selectNextRoi = useCallback(() => {
        const isCurrentIndexAlreadySelected =
            selectedRoiIds.length === 1 &&
            rois[currentIndex] &&
            selectedRoiIds[0] === rois[currentIndex].id;

        let nextIndex;

        if (isCurrentIndexAlreadySelected) {
            nextIndex = getNextIndex(currentIndex, rois.length);
        } else if (selectedRoiIds.length === 1) {
            const selectedRoiId = selectedRoiIds[0];
            const foundIndex = rois.findIndex(
                (roi) => roi.id === selectedRoiId
            );
            nextIndex = getNextIndex(foundIndex, rois.length);
        } else {
            nextIndex = getNextIndex(-1, rois.length);
        }

        setCurrentIndex(nextIndex);
        setSelectedRoiIds([rois[nextIndex].id]);
    }, [currentIndex, rois, selectedRoiIds, setSelectedRoiIds]);

    return { selectNextRoi };
};
