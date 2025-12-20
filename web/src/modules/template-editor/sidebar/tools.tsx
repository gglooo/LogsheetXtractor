import { Button } from "@/components/ui/button";
import { SidebarGroup } from "@/components/ui/sidebar";
import { DetectRoisAction } from "@/modules/rois/actions/detect-rois-action";
import type { DetectRoiResponseType } from "@/modules/rois/schema";
import { ShortcutLabel } from "@/modules/template-editor/components/shortcut-label";
import {
    CLEAR_ROIS_KEY,
    DRAW_TOOL_KEY,
    SELECT_TOOL_KEY,
    useKeyboardShortcuts,
} from "@/modules/template-editor/hooks/use-keyboard-shortcuts";
import { useSelectedRois } from "@/modules/template-editor/hooks/use-selected-rois";
import { useTemplateEditor } from "@/modules/template-editor/hooks/use-template-editor";
import { MousePointer, Square, X } from "lucide-react";
import { useCallback } from "react";
import { useIntl } from "react-intl";
import { useParams } from "react-router-dom";

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

    useKeyboardShortcuts({
        selectTool: setSelectTool,
        drawTool: setDrawTool,
        clearRois,
        undo,
        redo,
        selectAll,
        deleteTool,
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
