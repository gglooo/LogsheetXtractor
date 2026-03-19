import { getDuplicates } from "@/modules/pdf/utils";
import type {
    DetectedResidualType,
    ResidualType,
} from "@/modules/residuals/schema";
import type { RoiTypeEnum } from "@/modules/rois/roi-type-schema";
import type { DetectedRoiType, RoiType } from "@/modules/rois/schema";
import { useHistory } from "@/modules/template-editor/hooks/use-history";
import {
    TemplateEditorContext,
    type EditorMode,
} from "@/modules/template-editor/hooks/use-template-editor";
import { sortRoisByPosition } from "@/modules/template-editor/utils/roi";
import type { TemplateType } from "@/modules/templates/schema";
import type { Coordinates } from "@/schema";
import { useCallback, useMemo, useRef, useState, type ReactNode } from "react";
import { v4 as uuidv4 } from "uuid";

type EditorStateWithHistory = {
    rois: RoiType[];
    residuals: ResidualType[];
};

const DRAW_ROI_TYPES: RoiTypeEnum[] = [
    "Handwritten",
    "Number",
    "Checkbox",
    "Barcode",
];

const roundCoordinates = (coords: Coordinates): Coordinates => ({
    ...coords,
    x: Math.round(coords.x),
    y: Math.round(coords.y),
    width: Math.round(coords.width),
    height: Math.round(coords.height),
});

const addRequiredParamsToRoi = (
    roi: DetectedRoiType,
): RoiType => {
    return {
        ...roi,
        id: roi.id ?? uuidv4(),
        type: roi.type ?? "Handwritten",
        coordinates: roundCoordinates(roi.coordinates),
    };
};

const addRequiredParamsToResidual = (
    residual: DetectedResidualType,
): ResidualType => {
    return {
        ...residual,
        id: residual.id ?? uuidv4(),
    };
};

export const TemplateEditorProvider = ({
    template,
    children,
}: {
    template: TemplateType;
    children: ReactNode;
}) => {
    const [mode, setMode] = useState<EditorMode>("select");
    const [drawRoiType, setDrawRoiType] = useState<RoiTypeEnum>("Handwritten");
    const roiInputRef = useRef<HTMLInputElement>(null);

    const { state, set, undo, redo, canUndo, canRedo, isDirty, markAsSaved } =
        useHistory<EditorStateWithHistory>({
            rois: sortRoisByPosition(template?.rois ?? []),
            residuals: sortRoisByPosition(template?.residuals ?? []),
        });

    const rois = state.rois;
    const setRois = (rois: React.SetStateAction<DetectedRoiType[]>) => {
        set((prev) => {
            if (typeof rois === "function") {
                rois = rois(prev.rois);
            }

            return {
                ...prev,
                rois: sortRoisByPosition(rois.map(addRequiredParamsToRoi)),
            };
        });
    };
    const residuals = state.residuals;
    const setResiduals = (
        residuals: React.SetStateAction<DetectedResidualType[]>,
    ) => {
        set((prev) => {
            if (typeof residuals === "function") {
                residuals = residuals(prev.residuals);
            }

            return {
                ...prev,
                residuals: residuals.map(addRequiredParamsToResidual),
            };
        });
    };

    const setRoisAndResiduals = (
        rois: DetectedRoiType[],
        residuals: DetectedResidualType[],
    ) => {
        set((prev) => ({
            ...prev,
            rois: rois.map(addRequiredParamsToRoi),
            residuals: residuals.map(addRequiredParamsToResidual),
        }));
    };

    const getNewRoi = (
        coordinates: Coordinates,
        name?: string,
        type?: RoiTypeEnum,
        validationCondition?: RoiType["validationCondition"],
    ): RoiType => {
        if (!template) {
            throw new Error("Template is not defined");
        }

        const baseName = name ?? "unnamed_roi";
        const uniqueId = uuidv4();

        const newRoi: RoiType = {
            id: uniqueId,
            templateId: template.id,
            variableName: `${baseName}-${uniqueId.slice(0, 4)}`,
            type: type ?? drawRoiType,
            coordinates: roundCoordinates(coordinates),
            validationCondition: validationCondition ?? null,
            createdAt: new Date().toISOString(),
        };

        return newRoi;
    };

    const addRoi = (coordinates: Coordinates, name?: string, type?: RoiTypeEnum) => {
        const newRoi = getNewRoi(coordinates, name, type);

        setRois((prev) => [...prev, newRoi]);

        return newRoi.id!;
    };

    const addRois = (
        roisToAdd: {
            coordinates: Coordinates;
            name?: string;
            type?: RoiTypeEnum;
            validationCondition?: RoiType["validationCondition"];
        }[],
    ) => {
        const addedRoiIds: string[] = [];
        const newRois: RoiType[] = [];

        for (const roiData of roisToAdd) {
            const newRoi = getNewRoi(
                roiData.coordinates,
                roiData.name,
                roiData.type,
                roiData.validationCondition,
            );
            newRois.push(newRoi);
            addedRoiIds.push(newRoi.id!);
        }

        setRois((prev) => [...prev, ...newRois]);

        return addedRoiIds;
    };

    const removeRoi = (id: string) => {
        setRois((prev) => prev.filter((roi) => roi.id !== id));
    };

    const duplicateRoiNames = useMemo(
        () => getDuplicates(rois.map((r) => r.variableName)),
        [rois],
    );

    const cycleDrawRoiType = useCallback(() => {
        setDrawRoiType((previousType) => {
            const currentIndex = DRAW_ROI_TYPES.indexOf(previousType);
            const nextIndex = (currentIndex + 1) % DRAW_ROI_TYPES.length;
            return DRAW_ROI_TYPES[nextIndex];
        });
    }, []);

    return (
        <TemplateEditorContext.Provider
            value={{
                mode,
                setMode,
                drawRoiType,
                setDrawRoiType,
                cycleDrawRoiType,
                rois,
                setRois,
                residuals,
                setResiduals,
                setRoisAndResiduals,
                template,
                addRoi,
                addRois,
                getNewRoi,
                removeRoi,
                undo,
                redo,
                canUndo,
                canRedo,
                roiInputRef,
                duplicateRoiNames,
                isDirty,
                markAsSaved,
            }}
        >
            {children}
        </TemplateEditorContext.Provider>
    );
};
