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
    const { setSelectedRoiIds } = useSelectedRois();
    const { setRois } = useTemplateEditor();

    return useCallback(
        ({ preset, clickedRoiId }: ApplyValidationPresetParams) => {
            setSelectedRoiIds([clickedRoiId]);
            setRois((prevRois) => {
                return prevRois.map((roi) => {
                    if (roi.id !== clickedRoiId) {
                        return roi;
                    }

                    return {
                        ...roi,
                        type: preset.roiType,
                        validationCondition: cloneValidationCondition(
                            preset.condition,
                        ),
                    };
                });
            });
        },
        [setRois, setSelectedRoiIds],
    );
};
