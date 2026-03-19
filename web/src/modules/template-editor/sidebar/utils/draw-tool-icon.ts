import type { RoiTypeEnum } from "@/modules/rois/roi-type-schema";
import type { EditorMode } from "@/modules/template-editor/hooks/use-template-editor";
import type { LucideIcon } from "lucide-react";
import { Barcode, CheckSquare, Hash, Signature, Square } from "lucide-react";

export const getDrawToolIcon = (
    mode: EditorMode,
    drawRoiType: RoiTypeEnum,
): LucideIcon => {
    if (mode !== "draw") {
        return Square;
    }

    switch (drawRoiType) {
        case "Number":
            return Hash;
        case "Checkbox":
            return CheckSquare;
        case "Barcode":
            return Barcode;
        case "Handwritten":
        default:
            return Signature;
    }
};
