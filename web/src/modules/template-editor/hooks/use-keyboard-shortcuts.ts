import { useCallback, useEffect } from "react";

export const DRAW_TOOL_KEY = "d";
export const SELECT_TOOL_KEY = "s";
export const CLEAR_ROIS_KEY = "r";
export const UNDO_KEY = "z";
export const SELECT_ALL_KEY = "a";
export const DELETE_KEY = "Delete";

export const useKeyboardShortcuts = ({
    selectTool,
    drawTool,
    clearRois,
    undo,
    redo,
    selectAll,
    deleteTool,
}: {
    selectTool: () => void;
    drawTool: () => void;
    clearRois: () => void;
    undo: () => void;
    redo: () => void;
    selectAll: () => void;
    deleteTool: () => void;
}) => {
    const handleKeyDown = useCallback(
        (event: KeyboardEvent) => {
            if (
                event.target instanceof HTMLInputElement ||
                event.target instanceof HTMLTextAreaElement
            ) {
                return;
            }

            const key = event.key;
            const isCtrl = event.ctrlKey || event.metaKey;

            if (isCtrl) {
                if (key === UNDO_KEY && !event.shiftKey) {
                    event.preventDefault();
                    undo();
                    return;
                }
                if (key === UNDO_KEY && event.shiftKey) {
                    event.preventDefault();
                    redo();
                    return;
                }
                if (key === SELECT_ALL_KEY) {
                    event.preventDefault();
                    selectAll();
                    return;
                }
                return;
            }

            switch (event.key) {
                case SELECT_TOOL_KEY:
                case SELECT_TOOL_KEY.toUpperCase():
                    selectTool();
                    event.preventDefault();
                    break;
                case DRAW_TOOL_KEY:
                case DRAW_TOOL_KEY.toUpperCase():
                    drawTool();
                    event.preventDefault();
                    break;
                case CLEAR_ROIS_KEY:
                case CLEAR_ROIS_KEY.toUpperCase():
                    clearRois();
                    event.preventDefault();
                    break;
                case DELETE_KEY:
                    deleteTool();
                    event.preventDefault();
                    break;
                default:
                    break;
            }
        },
        [undo, redo, selectAll, selectTool, drawTool, clearRois, deleteTool]
    );

    useEffect(() => {
        window.addEventListener("keydown", handleKeyDown);
        return () => {
            window.removeEventListener("keydown", handleKeyDown);
        };
    }, [handleKeyDown]);
};
