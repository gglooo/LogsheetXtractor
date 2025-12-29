import type {
    DetectedResidualType,
    ResidualType,
} from "@/modules/residuals/schema";
import type { DetectedRoiType, RoiType } from "@/modules/rois/schema";
import type { TemplateType } from "@/modules/templates/schema";
import type { Coordinates } from "@/schema";
import { createContext, useContext, type SetStateAction } from "react";

export type EditorMode = "draw" | "select";

export type TemplateEditorContextType = {
    mode: EditorMode;
    setMode: (mode: EditorMode) => void;
    rois: RoiType[];
    setRois: React.Dispatch<SetStateAction<DetectedRoiType[]>>;
    residuals: ResidualType[];
    setResiduals: React.Dispatch<SetStateAction<DetectedResidualType[]>>;
    setRoisAndResiduals: (
        rois: DetectedRoiType[],
        residuals: DetectedResidualType[]
    ) => void;
    addRoi: (coordinates: Coordinates, name?: string) => string | undefined;
    removeRoi: (variableName: string) => void;
    template?: TemplateType;
    undo: () => void;
    redo: () => void;
    canUndo: boolean;
    canRedo: boolean;
    roiInputRef: React.RefObject<HTMLInputElement | null>;
    duplicateRoiNames: Set<string>;
};

export const TemplateEditorContext = createContext<
    TemplateEditorContextType | undefined
>(undefined);

export const useTemplateEditor = (): TemplateEditorContextType => {
    const context = useContext(TemplateEditorContext);
    if (!context) {
        throw new Error(
            "useTemplateEditor must be used within a TemplateEditorProvider"
        );
    }
    return context;
};
