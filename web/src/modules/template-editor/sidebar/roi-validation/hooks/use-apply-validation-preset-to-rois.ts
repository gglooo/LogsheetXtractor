import type { PredefinedRoiValidationConditionType } from "@/modules/rois/validation/schema";
import { useSelectedRois } from "@/modules/template-editor/hooks/use-selected-rois";
import { useTemplateEditor } from "@/modules/template-editor/hooks/use-template-editor";
import { cloneValidationCondition } from "@/modules/template-editor/sidebar/roi-validation/utils";
import { useCallback } from "react";

type ApplyValidationPresetParams = {
    preset: PredefinedRoiValidationConditionType;
    clickedRoiId: string;
};

export const useApplyValidationPresetToRois = () => {
    const { selectedRoiIds, setSelectedRoiIds } = useSelectedRois();
    const { setRois } = useTemplateEditor();

    return useCallback(
        ({ preset, clickedRoiId }: ApplyValidationPresetParams) => {
            const targetRoiIds =
                selectedRoiIds.length > 0 ? selectedRoiIds : [clickedRoiId];

            const targetRoiIdSet = new Set(targetRoiIds);

            setSelectedRoiIds(targetRoiIds);
            setRois((prevRois) =>
                prevRois.map((roi) => {
                    if (!targetRoiIdSet.has(roi.id ?? "")) {
                        return roi;
                    }

                    return {
                        ...roi,
                        type: preset.roiType,
                        validationCondition: cloneValidationCondition(
                            preset.condition,
                        ),
                    };
                }),
            );
        },
        [selectedRoiIds, setRois, setSelectedRoiIds],
    );
};
