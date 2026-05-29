import {
    useShortcutRegistry,
    type ShortcutKey,
    type ShortcutWhitelist,
} from "@/modules/template-editor/hooks/shortcuts/types";
import { useCallback, useEffect } from "react";

const getEventString = (event: KeyboardEvent): ShortcutKey => {
    const parts = [];
    if (event.ctrlKey || event.metaKey) {
        parts.push("Ctrl");
    }
    if (event.shiftKey) {
        parts.push("Shift");
    }

    parts.push(event.key);

    return parts.join("+") as ShortcutKey;
};

const isKeyWhiteListed = (
    event: KeyboardEvent,
    shortcutWhitelist: ShortcutWhitelist,
) => {
    const activeElement = document.activeElement;
    if (!activeElement) {
        return false;
    }

    const allowedShortcuts = shortcutWhitelist[activeElement.id];
    const eventString = getEventString(event);

    return allowedShortcuts?.includes(eventString);
};

export const useKeyboardShortcuts = (
    actions: {
        select: () => void;
        draw: () => void;
        split: () => void;
        clear: () => void;
        undo: () => void;
        redo: () => void;
        selectAll: () => void;
        delete: () => void;
        copy: () => void;
        paste: () => void;
        cut: () => void;
        browse: () => void;
        focusRoiInput: () => void;
    },
    shortcutWhitelist: ShortcutWhitelist = {},
) => {
    const shortcutRegistry = useShortcutRegistry();
    const handleKeyDown = useCallback(
        (event: KeyboardEvent) => {
            const isInput =
                event.target instanceof HTMLInputElement ||
                event.target instanceof HTMLTextAreaElement;

            if (isInput && !isKeyWhiteListed(event, shortcutWhitelist)) {
                return;
            }

            const eventString = getEventString(event);

            const matchedShortcut = shortcutRegistry.find(
                (s) =>
                    s.keys.includes(eventString as ShortcutKey) ||
                    s.keys.includes(eventString.toLowerCase() as ShortcutKey),
            );

            if (matchedShortcut) {
                const actionFn =
                    actions[matchedShortcut.actionKey as keyof typeof actions];
                if (typeof actionFn === "function") {
                    event.preventDefault();
                    actionFn();
                }
            }
        },
        [shortcutWhitelist, shortcutRegistry, actions],
    );

    useEffect(() => {
        window.addEventListener("keydown", handleKeyDown);

        return () => {
            window.removeEventListener("keydown", handleKeyDown);
        };
    }, [handleKeyDown]);
};
