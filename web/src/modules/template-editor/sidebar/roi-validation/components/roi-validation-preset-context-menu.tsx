import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import type { PredefinedRoiValidationConditionType } from "@/modules/rois/validation/schema";
import { usePresetLabel } from "@/modules/template-editor/sidebar/roi-validation/hooks/use-preset-label";
import { useEffect, useMemo, useRef, useState } from "react";
import { useIntl } from "react-intl";

type RoiValidationPresetContextMenuProps = {
    open: boolean;
    x: number;
    y: number;
    isLoading: boolean;
    presets: PredefinedRoiValidationConditionType[];
    onSelectPreset: (preset: PredefinedRoiValidationConditionType) => void;
    onClose: () => void;
};

export const RoiValidationPresetContextMenu = ({
    open,
    x,
    y,
    isLoading,
    presets,
    onSelectPreset,
    onClose,
}: RoiValidationPresetContextMenuProps) => {
    const menuRef = useRef<HTMLDivElement | null>(null);
    const intl = useIntl();
    const getPresetLabel = usePresetLabel();
    const [searchValue, setSearchValue] = useState("");

    useEffect(() => {
        if (!open) {
            // eslint-disable-next-line react-hooks/set-state-in-effect
            setSearchValue("");
            return;
        }

        const handlePointerDown = (event: MouseEvent) => {
            if (!menuRef.current?.contains(event.target as Node)) {
                onClose();
            }
        };

        const handleEscape = (event: KeyboardEvent) => {
            if (event.key === "Escape") {
                onClose();
            }
        };

        window.addEventListener("mousedown", handlePointerDown);
        window.addEventListener("contextmenu", handlePointerDown);
        window.addEventListener("keydown", handleEscape);

        return () => {
            window.removeEventListener("mousedown", handlePointerDown);
            window.removeEventListener("contextmenu", handlePointerDown);
            window.removeEventListener("keydown", handleEscape);
        };
    }, [onClose, open]);

    const filteredPresets = useMemo(() => {
        const normalizedSearch = searchValue.trim().toLowerCase();

        if (!normalizedSearch) {
            return presets;
        }

        return presets.filter((preset) => {
            const translatedLabel = getPresetLabel(preset.code, preset.label);
            return (
                translatedLabel.toLowerCase().includes(normalizedSearch) ||
                preset.label.toLowerCase().includes(normalizedSearch) ||
                preset.code.toLowerCase().includes(normalizedSearch)
            );
        });
    }, [getPresetLabel, searchValue, presets]);

    if (!open) {
        return null;
    }

    if (isLoading) {
        return (
            <div
                ref={menuRef}
                className="fixed w-56 rounded-md border bg-popover shadow-lg py-1 z-50"
                style={{ left: x, top: y }}
            >
                <div className="px-3 py-2 text-sm text-muted-foreground">
                    {intl.formatMessage({
                        id: "template-editor.roi-validation.context-menu.loading",
                        defaultMessage: "Loading presets...",
                    })}
                </div>
            </div>
        );
    }

    return (
        <div
            ref={menuRef}
            className="fixed w-56 rounded-md border bg-popover shadow-lg py-1 z-50"
            style={{ left: x, top: y }}
            onMouseDown={(e) => e.stopPropagation()}
        >
            <div className="px-3 py-1 text-xs font-semibold text-muted-foreground">
                {intl.formatMessage({
                    id: "template-editor.roi-validation.context-menu.title",
                    defaultMessage: "Validation preset",
                })}
            </div>

            <div className="px-2 pb-1">
                <Input
                    value={searchValue}
                    autoFocus
                    onChange={(e) => setSearchValue(e.target.value)}
                    className="h-8 text-xs"
                    placeholder={intl.formatMessage({
                        id: "template-editor.roi-validation.context-menu.search.placeholder",
                        defaultMessage: "Search presets...",
                    })}
                />
            </div>

            {presets.length === 0 ? (
                <div className="px-3 py-2 text-sm text-muted-foreground">
                    {intl.formatMessage({
                        id: "template-editor.roi-validation.context-menu.empty",
                        defaultMessage: "No validation presets available.",
                    })}
                </div>
            ) : (
                <div className="max-h-56 overflow-y-auto">
                    {filteredPresets.map((preset) => (
                        <Button
                            variant="ghost"
                            type="button"
                            key={preset.id}
                            className="w-full px-3 py-2 text-left text-sm justify-start hover:bg-accent disabled:opacity-50"
                            onClick={() => onSelectPreset(preset)}
                        >
                            {getPresetLabel(preset.code, preset.label)}
                        </Button>
                    ))}
                </div>
            )}

            {presets.length > 0 && filteredPresets.length === 0 ? (
                <div className="px-3 py-2 text-sm text-muted-foreground">
                    {intl.formatMessage({
                        id: "template-editor.roi-validation.context-menu.no-results",
                        defaultMessage: "No presets match your search.",
                    })}
                </div>
            ) : null}
        </div>
    );
};
