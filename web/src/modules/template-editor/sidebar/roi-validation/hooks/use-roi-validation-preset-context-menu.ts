import type { PredefinedRoiValidationConditionType } from "@/modules/rois/validation/schema";
import { usePredefinedRoiValidationConditions } from "@/modules/rois/validation/api";
import { useApplyValidationPresetToRois } from "@/modules/template-editor/sidebar/roi-validation/hooks/use-apply-validation-preset-to-rois";
import type { MouseEvent } from "react";
import { useCallback, useMemo, useState } from "react";

type PresetContextMenuState = {
    x: number;
    y: number;
    roiId: string;
};

export const useRoiValidationPresetContextMenu = (editable: boolean) => {
    const applyValidationPresetToRois = useApplyValidationPresetToRois();
    const predefinedConditionsQuery = usePredefinedRoiValidationConditions();
    const [presetContextMenu, setPresetContextMenu] =
        useState<PresetContextMenuState | null>(null);

    const handleOpenRoiContextMenu = useCallback(
        (e: MouseEvent, roiId: string) => {
            e.preventDefault();
            e.stopPropagation();
            if (!editable) {
                return;
            }

            setPresetContextMenu({
                x: e.clientX,
                y: e.clientY,
                roiId,
            });
        },
        [editable],
    );

    const handleClose = useCallback(() => {
        setPresetContextMenu(null);
    }, []);

    const handleSelectPreset = useCallback(
        (preset: PredefinedRoiValidationConditionType) => {
            if (!presetContextMenu?.roiId) {
                return;
            }

            applyValidationPresetToRois({
                preset,
                clickedRoiId: presetContextMenu.roiId,
            });
            setPresetContextMenu(null);
        },
        [applyValidationPresetToRois, presetContextMenu],
    );

    const menuProps = useMemo(
        () => ({
            open: !!presetContextMenu,
            x: presetContextMenu?.x ?? 0,
            y: presetContextMenu?.y ?? 0,
            isLoading: predefinedConditionsQuery.isLoading,
            presets: predefinedConditionsQuery.data ?? [],
            onClose: handleClose,
            onSelectPreset: handleSelectPreset,
        }),
        [
            handleClose,
            handleSelectPreset,
            predefinedConditionsQuery.data,
            predefinedConditionsQuery.isLoading,
            presetContextMenu,
        ],
    );

    return {
        handleOpenRoiContextMenu,
        menuProps,
    };
};
