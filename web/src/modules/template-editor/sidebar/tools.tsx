import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import { SidebarGroup } from "@/components/ui/sidebar";
import { DetectRoisAction } from "@/modules/rois/actions/detect-rois-action";
import type { DetectRoiResponseType, RoiType } from "@/modules/rois/schema";
import { ShortcutLabel } from "@/modules/template-editor/components/shortcut-label";
import {
    BROWSE_ROI_FORCE_KEY,
    CLEAR_ROIS_KEY,
    DRAW_TOOL_KEY,
    SELECT_TOOL_KEY,
    SPLIT_TOOL_KEY,
    type ShortcutWhitelist,
} from "@/modules/template-editor/hooks/shortcuts/types";
import { useKeyboardShortcuts } from "@/modules/template-editor/hooks/shortcuts/use-keyboard-shortcuts";
import { useBrowseSelectedRois } from "@/modules/template-editor/hooks/use-browse-selected-rois";
import { useSelectedRois } from "@/modules/template-editor/hooks/use-selected-rois";
import { useTemplateEditor } from "@/modules/template-editor/hooks/use-template-editor";
import { ShortcutTooltip } from "@/modules/template-editor/sidebar/components/shortcut-tooltip";
import { VARIABLE_NAME_INPUT_ID } from "@/modules/template-editor/sidebar/selected-roi";
import { copy, paste } from "@/modules/template-editor/utils/copy-paste";
import { Divide, MousePointer, Square, X } from "lucide-react";
import { useCallback } from "react";
import { useIntl } from "react-intl";
import { useParams } from "react-router-dom";

const PASTE_OFFSET = 80;

const shortcutWhitelist: ShortcutWhitelist = {
    [VARIABLE_NAME_INPUT_ID]: [BROWSE_ROI_FORCE_KEY],
};

const adjustRoiAfterPaste = (rois: RoiType[]) => {
    return rois.map((roi) => {
        return {
            ...roi,
            coordinates: {
                ...roi.coordinates,
                x: roi.coordinates.x + PASTE_OFFSET,
                y: roi.coordinates.y + PASTE_OFFSET,
            },
            variableName: `${roi.variableName}_copy`,
        };
    });
};

export const ToolsSidebarGroup = () => {
    const intl = useIntl();

    const { id } = useParams<{ id: string }>();

    const {
        addRoi,
        setRois,
        setRoisAndResiduals,
        setMode,
        mode,
        rois,
        residuals,
        undo,
        redo,
        roiInputRef,
    } = useTemplateEditor();
    const { setSelectedRoiIds, isSelectedRoi } = useSelectedRois();

    const handleSetDetectedData = (detectedData: DetectRoiResponseType) => {
        setRoisAndResiduals(
            [...rois, ...detectedData.rois],
            [...residuals, ...detectedData.residuals]
        );
    };

    const { selectNextRoi } = useBrowseSelectedRois();

    const setSelectTool = useCallback(() => setMode("select"), [setMode]);

    const setDrawTool = useCallback(() => setMode("draw"), [setMode]);

    const setSplitTool = useCallback(() => setMode("split"), [setMode]);

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

        const addedRoisIds = [];
        for (const roi of adjustedRois) {
            addedRoisIds.push(addRoi(roi.coordinates, roi.variableName)!);
        }

        setSelectedRoiIds(addedRoisIds);
        setMode("select");
    }, [addRoi, setMode, setSelectedRoiIds]);

    const cutTool = useCallback(() => {
        const roisToCut = rois.filter((roi) => isSelectedRoi(roi.id ?? ""));
        copy(roisToCut);
        deleteTool();
    }, [deleteTool, isSelectedRoi, rois]);

    const browseRoiTool = useCallback(() => {
        selectNextRoi();
    }, [selectNextRoi]);

    const focusRoiInput = useCallback(() => {
        roiInputRef.current?.focus();
    }, [roiInputRef]);

    useKeyboardShortcuts(
        {
            select: setSelectTool,
            draw: setDrawTool,
            split: setSplitTool,
            clear: clearRois,
            undo,
            redo,
            selectAll: selectAll,
            delete: deleteTool,
            copy: copyTool,
            paste: pasteTool,
            cut: cutTool,
            browse: browseRoiTool,
            focusRoiInput,
        },
        shortcutWhitelist
    );

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
                    variant={mode === "split" ? "default" : "outline"}
                    disabled={rois.length === 0}
                    className="w-full"
                    onClick={setSplitTool}
                >
                    <Divide />
                    <ShortcutLabel
                        shortcut={SPLIT_TOOL_KEY}
                        label={intl.formatMessage({
                            id: "templateEditor.sidebar.split",
                            defaultMessage: "Split ROI",
                        })}
                    />
                </Button>
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
                <Separator />
                <ShortcutTooltip />
            </div>
        </SidebarGroup>
    );
};
