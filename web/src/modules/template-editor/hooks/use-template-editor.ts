import type { DetectedResidualType } from "@/modules/residuals/schema";
import type { DetectedRoiType } from "@/modules/rois/schema";
import type { TemplateType } from "@/modules/templates/schema";
import type { Coordinates } from "@/schema";
import { createContext, useContext, type SetStateAction } from "react";

export type EditorMode = "draw" | "select";

export type TemplateEditorContextType = {
    mode: EditorMode;
    setMode: (mode: EditorMode) => void;
    rois: DetectedRoiType[];
    setRois: React.Dispatch<SetStateAction<DetectedRoiType[]>>;
    residuals: DetectedResidualType[];
    setResiduals: React.Dispatch<SetStateAction<DetectedResidualType[]>>;
    addRoi: (coordinates: Coordinates) => void;
    template?: TemplateType;
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
