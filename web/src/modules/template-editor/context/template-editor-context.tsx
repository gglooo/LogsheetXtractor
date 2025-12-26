import type {
    DetectedResidualType,
    ResidualType,
} from "@/modules/residuals/schema";
import type { DetectedRoiType, RoiType } from "@/modules/rois/schema";
import { useHistory } from "@/modules/template-editor/hooks/use-history";
import {
    TemplateEditorContext,
    type EditorMode,
} from "@/modules/template-editor/hooks/use-template-editor";
import { sortRoisByPosition } from "@/modules/template-editor/utils/roi";
import type { TemplateType } from "@/modules/templates/schema";
import type { Coordinates } from "@/schema";
import { useState, type ReactNode } from "react";

type EditorStateWithHistory = {
    rois: RoiType[];
    residuals: ResidualType[];
};

const addRequiredParamsToRoi = (roi: DetectedRoiType): RoiType => {
    return {
        ...roi,
        id: roi.id ?? crypto.randomUUID(),
        type: roi.type ?? "Handwritten",
    };
};

const addRequiredParamsToResidual = (
    residual: DetectedResidualType
): ResidualType => {
    return {
        ...residual,
        id: residual.id ?? crypto.randomUUID(),
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

    const { state, set, undo, redo, canUndo, canRedo } =
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
        residuals: React.SetStateAction<DetectedResidualType[]>
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
        residuals: DetectedResidualType[]
    ) => {
        set((prev) => ({
            ...prev,
            rois: rois.map(addRequiredParamsToRoi),
            residuals: residuals.map(addRequiredParamsToResidual),
        }));
    };

    const addRoi = (coordinates: Coordinates) => {
        if (!template) {
            return;
        }

        const newRoi: RoiType = {
            id: crypto.randomUUID(),
            templateId: template.id,
            variableName: "unnamed_roi",
            type: "Handwritten",
            coordinates,
            createdAt: new Date().toISOString(),
        };

        setRois((prev) => [...prev, newRoi]);

        return newRoi.id!;
    };

    const removeRoi = (id: string) => {
        setRois((prev) => prev.filter((roi) => roi.id !== id));
    };

    return (
        <TemplateEditorContext.Provider
            value={{
                mode,
                setMode,
                rois,
                setRois,
                residuals,
                setResiduals,
                setRoisAndResiduals,
                template,
                addRoi,
                removeRoi,
                undo,
                redo,
                canUndo,
                canRedo,
            }}
        >
            {children}
        </TemplateEditorContext.Provider>
    );
};
