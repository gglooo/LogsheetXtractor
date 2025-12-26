export type ShortcutKey =
    | "Tab"
    | "Shift+Tab"
    | "d"
    | "s"
    | "r"
    | "f"
    | "Ctrl+z"
    | "Ctrl+Shift+z"
    | "Ctrl+a"
    | "Delete"
    | "Ctrl+c"
    | "Ctrl+v"
    | "Ctrl+x";

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
export const CLEAR_ROIS_KEY: ShortcutKey = "r";
export const FOCUS_ROI_INPUT_KEY: ShortcutKey = "f";

export const SHORTCUT_REGISTRY: readonly ShortcutConfig[] = [
    {
        name: "Browse ROIs",
        description: "Browse through ROIs",
        keys: [TAB_KEY, BROWSE_ROI_FORCE_KEY],
        actionKey: "browse",
    },
    {
        name: "Draw Tool",
        description: "Activate the draw tool",
        keys: [DRAW_TOOL_KEY],
        actionKey: "draw",
    },
    {
        name: "Select Tool",
        description: "Activate the select tool",
        keys: [SELECT_TOOL_KEY],
        actionKey: "select",
    },
    {
        name: "Undo",
        description: "Undo last action",
        keys: [`${CONTROL_KEY}+z`],
        actionKey: "undo",
    },
    {
        name: "Redo",
        description: "Redo last undone action",
        keys: [`${CONTROL_KEY}+${SHIFT_KEY}+z`],
        actionKey: "redo",
    },
    {
        name: "Select All",
        description: "Select all ROIs",
        keys: [`${CONTROL_KEY}+a`],
        actionKey: "selectAll",
    },
    {
        name: "Clear ROIs",
        description: "Clear all ROIs",
        keys: [CLEAR_ROIS_KEY],
        actionKey: "clear",
    },
    {
        name: "Delete",
        description: "Delete selected ROIs",
        keys: ["Delete"],
        actionKey: "delete",
    },
    {
        name: "Copy",
        description: "Copy selected ROIs",
        keys: [`${CONTROL_KEY}+c`],
        actionKey: "copy",
    },
    {
        name: "Paste",
        description: "Paste copied ROIs",
        keys: [`${CONTROL_KEY}+v`],
        actionKey: "paste",
    },
    {
        name: "Cut",
        description: "Cut selected ROIs",
        keys: [`${CONTROL_KEY}+x`],
        actionKey: "cut",
    },
    {
        name: "Focus ROI Input",
        description: "Focus the ROI input field",
        keys: [FOCUS_ROI_INPUT_KEY],
        actionKey: "focusRoiInput",
    },
] as const;

export type ShortcutWhitelist = Record<string, string[]>;
