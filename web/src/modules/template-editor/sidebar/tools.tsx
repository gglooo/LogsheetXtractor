import { Button } from "@/components/ui/button";
import { SidebarGroup } from "@/components/ui/sidebar";
import { DetectRoisAction } from "@/modules/rois/actions/detect-rois-action";
import type { DetectRoiResponseType, RoiType } from "@/modules/rois/schema";
import { ShortcutLabel } from "@/modules/template-editor/components/shortcut-label";
import {
    CLEAR_ROIS_KEY,
    DRAW_TOOL_KEY,
    SELECT_TOOL_KEY,
    useKeyboardShortcuts,
} from "@/modules/template-editor/hooks/use-keyboard-shortcuts";
import { useSelectedRois } from "@/modules/template-editor/hooks/use-selected-rois";
import { useTemplateEditor } from "@/modules/template-editor/hooks/use-template-editor";
import { copy, paste } from "@/modules/template-editor/utils/copy-paste";
import { MousePointer, Square, X } from "lucide-react";
import { useCallback } from "react";
import { useIntl } from "react-intl";
import { useParams } from "react-router-dom";

const PASTE_OFFSET = 80;

const adjustRoiAfterPaste = (rois: RoiType[]) => {
    return rois.map((roi) => {
        return {
            ...roi,
            coordinates: {
                ...roi.coordinates,
                x: roi.coordinates.x + PASTE_OFFSET,
                y: roi.coordinates.y + PASTE_OFFSET,
            },
            id: crypto.randomUUID(),
        };
    });
};

export const ToolsSidebarGroup = () => {
    const intl = useIntl();

    const { id } = useParams<{ id: string }>();

    const {
        setRois,
        setRoisAndResiduals,
        setMode,
        mode,
        rois,
        residuals,
        undo,
        redo,
    } = useTemplateEditor();
    const { setSelectedRoiIds, isSelectedRoi } = useSelectedRois();

    const handleSetDetectedData = (detectedData: DetectRoiResponseType) => {
        setRoisAndResiduals(
            [...rois, ...detectedData.rois],
            [...residuals, ...detectedData.residuals]
        );
    };

    const setSelectTool = useCallback(() => setMode("select"), [setMode]);

    const setDrawTool = useCallback(() => setMode("draw"), [setMode]);

    const clearRois = useCallback(() => setRois([]), [setRois]);

    const selectAll = useCallback(() => {
        setSelectedRoiIds(rois.filter((roi) => roi.id).map((roi) => roi.id!));
    }, [rois, setSelectedRoiIds]);

    const deleteTool = useCallback(() => {
        setRois(rois.filter((roi) => !isSelectedRoi(roi.id ?? "")));
    }, [isSelectedRoi, rois, setRois]);

    const copyTool = useCallback(async () => {
        await copy(rois.filter((roi) => isSelectedRoi(roi.id ?? "")));
    }, [isSelectedRoi, rois]);

    const pasteTool = useCallback(async () => {
        const pastedRois = await paste<RoiType>();
        const adjustedRois = adjustRoiAfterPaste(pastedRois ?? []);
        if (!((pastedRois?.length ?? 0) > 0)) {
            return;
        }

        setRois([...rois, ...adjustedRois]);
        setSelectedRoiIds(adjustedRois.map((roi) => roi.id!));
        setMode("select");
    }, [rois, setMode, setRois, setSelectedRoiIds]);

    const cutTool = useCallback(() => {
        const roisToCut = rois.filter((roi) => isSelectedRoi(roi.id ?? ""));
        copy(roisToCut);
        deleteTool();
    }, [deleteTool, isSelectedRoi, rois]);

    useKeyboardShortcuts({
        selectTool: setSelectTool,
        drawTool: setDrawTool,
        clearRois,
        undo,
        redo,
        selectAll,
        deleteTool,
        copyTool,
        pasteTool,
        cutTool,
    });

    return (
        <SidebarGroup
            title={intl.formatMessage({
                id: "templateEditor.sidebar.tools",
                defaultMessage: "Tools",
            })}
        >
            <div className="flex flex-col gap-2 p-2 w-full">
                <div className="flex flex-row gap-2 flex-1">
                    <Button
                        className="flex-1"
                        variant={mode === "select" ? "default" : "outline"}
                        onClick={setSelectTool}
                    >
                        <MousePointer />
                        <ShortcutLabel
                            shortcut={SELECT_TOOL_KEY}
                            label={intl.formatMessage({
                                id: "templateEditor.sidebar.selectTool",
                                defaultMessage: "Select",
                            })}
                        />
                    </Button>
                    <Button
                        className="flex-1"
                        variant={mode === "draw" ? "default" : "outline"}
                        onClick={setDrawTool}
                    >
                        <Square />
                        <ShortcutLabel
                            shortcut={DRAW_TOOL_KEY}
                            label={intl.formatMessage({
                                id: "templateEditor.sidebar.drawTool",
                                defaultMessage: "Draw",
                            })}
                        />
                    </Button>
                </div>
                <Button
                    variant="outline"
                    disabled={rois.length === 0}
                    className="w-full"
                    onClick={clearRois}
                >
                    <X />
                    <ShortcutLabel
                        shortcut={CLEAR_ROIS_KEY}
                        label={intl.formatMessage({
                            id: "templateEditor.sidebar.clearRois",
                            defaultMessage: "Clear ROIs",
                        })}
                    />
                </Button>
                <DetectRoisAction
                    templateId={id!}
                    onResult={handleSetDetectedData}
                    className="flex-1"
                />
            </div>
        </SidebarGroup>
    );
};
