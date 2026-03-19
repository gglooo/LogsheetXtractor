import type {
    DetectedResidualType,
    ResidualType,
} from "@/modules/residuals/schema";
import type { RoiTypeEnum } from "@/modules/rois/roi-type-schema";
import type { DetectedRoiType, RoiType } from "@/modules/rois/schema";
import type { TemplateType } from "@/modules/templates/schema";
import type { Coordinates } from "@/schema";
import { createContext, useContext, type SetStateAction } from "react";

export type EditorMode = "draw" | "select" | "split";

export type TemplateEditorContextType = {
    mode: EditorMode;
    setMode: (mode: EditorMode) => void;
    rois: RoiType[];
    setRois: React.Dispatch<SetStateAction<DetectedRoiType[]>>;
    residuals: ResidualType[];
    setResiduals: React.Dispatch<SetStateAction<DetectedResidualType[]>>;
    setRoisAndResiduals: (
        rois: DetectedRoiType[],
        residuals: DetectedResidualType[],
    ) => void;
    addRoi: (
        coordinates: Coordinates,
        name?: string,
        type?: RoiTypeEnum,
    ) => string | undefined;
    addRois: (
        rois: {
            coordinates: Coordinates;
            name?: string;
            type?: RoiTypeEnum;
            validationCondition?: RoiType["validationCondition"];
        }[],
    ) => string[];
    getNewRoi: (
        coordinates: Coordinates,
        name?: string,
        type?: RoiTypeEnum,
        validationCondition?: RoiType["validationCondition"],
    ) => RoiType;
    removeRoi: (variableName: string) => void;
    drawRoiType: RoiTypeEnum;
    setDrawRoiType: (type: RoiTypeEnum) => void;
    cycleDrawRoiType: () => void;
    template?: TemplateType;
    undo: () => void;
    redo: () => void;
    canUndo: boolean;
    canRedo: boolean;
    roiInputRef: React.RefObject<HTMLInputElement | null>;
    duplicateRoiNames: Set<string>;
    isDirty: boolean;
    markAsSaved: () => void;
};

export const TemplateEditorContext = createContext<
    TemplateEditorContextType | undefined
>(undefined);

export const useTemplateEditor = (): TemplateEditorContextType => {
    const context = useContext(TemplateEditorContext);
    if (!context) {
        throw new Error(
            "useTemplateEditor must be used within a TemplateEditorProvider",
        );
    }
    return context;
};
