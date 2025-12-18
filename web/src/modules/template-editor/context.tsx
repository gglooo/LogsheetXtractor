import type { DetectedResidualType } from "@/modules/residuals/schema";
import type { DetectedRoiType } from "@/modules/rois/schema";
import {
    TemplateEditorContext,
    type EditorMode,
} from "@/modules/template-editor/hooks/use-template-editor";
import type { TemplateType } from "@/modules/templates/schema";
import type { Coordinates } from "@/schema";
import { useState, type ReactNode } from "react";

export const TemplateEditorProvider = ({
    template,
    children,
}: {
    template: TemplateType | undefined;
    children: ReactNode;
}) => {
    const [mode, setMode] = useState<EditorMode>("select");
    const [rois, setRois] = useState<DetectedRoiType[]>([]);
    const [residuals, setResiduals] = useState<DetectedResidualType[]>([]);

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
                template,
                addRoi,
            }}
        >
            {children}
        </TemplateEditorContext.Provider>
    );
};
