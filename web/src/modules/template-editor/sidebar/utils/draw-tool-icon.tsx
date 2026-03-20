import type { RoiTypeEnum } from "@/modules/rois/roi-type-schema";
import type { EditorMode } from "@/modules/template-editor/hooks/use-template-editor";
import { Barcode, CheckSquare, Hash, Signature, Square } from "lucide-react";

type DrawToolIconProps = {
    mode: EditorMode;
    drawRoiType: RoiTypeEnum;
};

export const DrawToolIcon = ({ mode, drawRoiType }: DrawToolIconProps) => {
    if (mode !== "draw") {
        return <Square />;
    }

    switch (drawRoiType) {
        case "Number":
            return <Hash />;
        case "Checkbox":
            return <CheckSquare />;
        case "Barcode":
            return <Barcode />;
        case "Handwritten":
        default:
            return <Signature />;
    }
};
