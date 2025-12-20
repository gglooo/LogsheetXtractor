import type { DetectedResidualType } from "@/modules/residuals/schema";
import type { DetectedRoiType } from "@/modules/rois/schema";
import { useHistory } from "@/modules/template-editor/hooks/use-history";
import {
    TemplateEditorContext,
    type EditorMode,
} from "@/modules/template-editor/hooks/use-template-editor";
import type { TemplateType } from "@/modules/templates/schema";
import type { Coordinates } from "@/schema";
import { useState, type ReactNode } from "react";

type EditorStateWithHistory = {
    rois: DetectedRoiType[];
    residuals: DetectedResidualType[];
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
            rois: template?.rois ?? [],
            residuals: template?.residuals ?? [],
        });

    const rois = state.rois;
    const setRois = (rois: React.SetStateAction<DetectedRoiType[]>) => {
        set((prev) => ({
            ...prev,
            rois: typeof rois === "function" ? rois(prev.rois) : rois,
        }));
    };
    const residuals = state.residuals;
    const setResiduals = (
        residuals: React.SetStateAction<DetectedResidualType[]>
    ) => {
        set((prev) => ({
            ...prev,
            residuals:
                typeof residuals === "function"
                    ? residuals(prev.residuals)
                    : residuals,
        }));
    };

    const setRoisAndResiduals = (
        rois: DetectedRoiType[],
        residuals: DetectedResidualType[]
    ) => {
        set((prev) => ({
            ...prev,
            rois,
            residuals,
        }));
    };

    const addRoi = (coordinates: Coordinates) => {
        if (!template) {
            return;
        }

        const newRoi: DetectedRoiType = {
            id: `roi-${Date.now()}`,
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
