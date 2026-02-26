import { useMemo } from "react";
import { useIntl } from "react-intl";

export type ShortcutKey =
    | "Tab"
    | "Shift+Tab"
    | "d"
    | "s"
    | "t"
    | "r"
    | "f"
    | "Ctrl+z"
    | "Ctrl+Shift+z"
    | "Ctrl+a"
    | "Delete"
    | "Ctrl+c"
    | "Ctrl+v"
    | "Ctrl+x"
    | "Ctrl+t";

export type ShortcutConfig = {
    name: string;
    description: string;
    keys: ShortcutKey[];
    actionKey: string;
};

export const CONTROL_KEY = "Ctrl";
export const SHIFT_KEY = "Shift";
export const TAB_KEY = "Tab";
export const BROWSE_ROI_FORCE_KEY: ShortcutKey = `${SHIFT_KEY}+${TAB_KEY}`;
export const DRAW_TOOL_KEY: ShortcutKey = "d";
export const SELECT_TOOL_KEY: ShortcutKey = "s";
export const SPLIT_TOOL_KEY: ShortcutKey = "t";
export const CLEAR_ROIS_KEY: ShortcutKey = "r";
export const FOCUS_ROI_INPUT_KEY: ShortcutKey = "f";

export const useShortcutRegistry = (): readonly ShortcutConfig[] => {
    const intl = useIntl();
    return useMemo(
        () => [
            {
                name: intl.formatMessage({
                    id: "shortcut.browseROIs",
                    defaultMessage: "Browse ROIs",
                }),
                description: intl.formatMessage({
                    id: "shortcut.browseROIs.description",
                    defaultMessage: "Browse through ROIs",
                }),
                keys: [TAB_KEY, BROWSE_ROI_FORCE_KEY],
                actionKey: "browse",
            },
            {
                name: intl.formatMessage({
                    id: "shortcut.drawTool",
                    defaultMessage: "Draw Tool",
                }),
                description: intl.formatMessage({
                    id: "shortcut.drawTool.description",
                    defaultMessage: "Activate the draw tool",
                }),
                keys: [DRAW_TOOL_KEY],
                actionKey: "draw",
            },
            {
                name: intl.formatMessage({
                    id: "shortcut.selectTool",
                    defaultMessage: "Select Tool",
                }),
                description: intl.formatMessage({
                    id: "shortcut.selectTool.description",
                    defaultMessage: "Activate the select tool",
                }),
                keys: [SELECT_TOOL_KEY],
                actionKey: "select",
            },
            {
                name: intl.formatMessage({
                    id: "shortcut.splitTool",
                    defaultMessage: "Split tool",
                }),
                description: intl.formatMessage({
                    id: "shortcut.splitTool.description",
                    defaultMessage:
                        "Click on ROI to split it ctrl + click to split in opposite direction",
                }),
                keys: [SPLIT_TOOL_KEY],
                actionKey: "split",
            },
            {
                name: intl.formatMessage({
                    id: "shortcut.undo",
                    defaultMessage: "Undo",
                }),
                description: intl.formatMessage({
                    id: "shortcut.undo.description",
                    defaultMessage: "Undo last action",
                }),
                keys: [`${CONTROL_KEY}+z`],
                actionKey: "undo",
            },
            {
                name: intl.formatMessage({
                    id: "shortcut.redo",
                    defaultMessage: "Redo",
                }),
                description: intl.formatMessage({
                    id: "shortcut.redo.description",
                    defaultMessage: "Redo last undone action",
                }),
                keys: [`${CONTROL_KEY}+${SHIFT_KEY}+z`],
                actionKey: "redo",
            },
            {
                name: intl.formatMessage({
                    id: "shortcut.selectAll",
                    defaultMessage: "Select All",
                }),
                description: intl.formatMessage({
                    id: "shortcut.selectAll.description",
                    defaultMessage: "Select all ROIs",
                }),
                keys: [`${CONTROL_KEY}+a`],
                actionKey: "selectAll",
            },
            {
                name: intl.formatMessage({
                    id: "shortcut.clearROIs",
                    defaultMessage: "Clear ROIs",
                }),
                description: intl.formatMessage({
                    id: "shortcut.clearROIs.description",
                    defaultMessage: "Clear all ROIs",
                }),
                keys: [CLEAR_ROIS_KEY],
                actionKey: "clear",
            },
            {
                name: intl.formatMessage({
                    id: "shortcut.delete",
                    defaultMessage: "Delete",
                }),
                description: intl.formatMessage({
                    id: "shortcut.delete.description",
                    defaultMessage: "Delete selected ROIs",
                }),
                keys: ["Delete"],
                actionKey: "delete",
            },
            {
                name: intl.formatMessage({
                    id: "shortcut.copy",
                    defaultMessage: "Copy",
                }),
                description: intl.formatMessage({
                    id: "shortcut.copy.description",
                    defaultMessage: "Copy selected ROIs",
                }),
                keys: [`${CONTROL_KEY}+c`],
                actionKey: "copy",
            },
            {
                name: intl.formatMessage({
                    id: "shortcut.paste",
                    defaultMessage: "Paste",
                }),
                description: intl.formatMessage({
                    id: "shortcut.paste.description",
                    defaultMessage: "Paste copied ROIs",
                }),
                keys: [`${CONTROL_KEY}+v`],
                actionKey: "paste",
            },
            {
                name: intl.formatMessage({
                    id: "shortcut.cut",
                    defaultMessage: "Cut",
                }),
                description: intl.formatMessage({
                    id: "shortcut.cut.description",
                    defaultMessage: "Cut selected ROIs",
                }),
                keys: [`${CONTROL_KEY}+x`],
                actionKey: "cut",
            },
            {
                name: intl.formatMessage({
                    id: "shortcut.focusRoiInput",
                    defaultMessage: "Focus ROI Input",
                }),
                description: intl.formatMessage({
                    id: "shortcut.focusRoiInput.description",
                    defaultMessage: "Focus the ROI input field",
                }),
                keys: [FOCUS_ROI_INPUT_KEY],
                actionKey: "focusRoiInput",
            },
        ],
        [intl],
    );
};

export type ShortcutWhitelist = Record<string, string[]>;
