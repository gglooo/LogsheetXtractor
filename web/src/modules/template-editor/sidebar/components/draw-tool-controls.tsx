import { Button } from "@/components/ui/button";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuLabel,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { ShortcutLabel } from "@/modules/template-editor/components/shortcut-label";
import { DRAW_TOOL_KEY } from "@/modules/template-editor/hooks/shortcuts/types";
import type {
    EditorMode,
    TemplateEditorContextType,
} from "@/modules/template-editor/hooks/use-template-editor";
import { getDrawToolIcon } from "@/modules/template-editor/sidebar/utils/draw-tool-icon";
import { Check, ChevronDown } from "lucide-react";
import { useIntl } from "react-intl";

type DrawToolControlsProps = {
    mode: EditorMode;
    isEditable: boolean;
    drawRoiType: TemplateEditorContextType["drawRoiType"];
    setDrawRoiType: TemplateEditorContextType["setDrawRoiType"];
    setDrawTool: () => void;
};

export const DrawToolControls = ({
    mode,
    isEditable,
    drawRoiType,
    setDrawRoiType,
    setDrawTool,
}: DrawToolControlsProps) => {
    const intl = useIntl();
    const DrawToolIcon = getDrawToolIcon(mode, drawRoiType);

    const drawTypeOptions = [
        {
            value: "Handwritten" as const,
            label: intl.formatMessage({
                id: "templateEditor.sidebar.drawSubtool.handwritten",
                defaultMessage: "Handwritten",
            }),
        },
        {
            value: "Number" as const,
            label: intl.formatMessage({
                id: "templateEditor.sidebar.drawSubtool.number",
                defaultMessage: "Numeric",
            }),
        },
        {
            value: "Checkbox" as const,
            label: intl.formatMessage({
                id: "templateEditor.sidebar.drawSubtool.checkbox",
                defaultMessage: "Checkbox",
            }),
        },
        {
            value: "Barcode" as const,
            label: intl.formatMessage({
                id: "templateEditor.sidebar.drawSubtool.barcode",
                defaultMessage: "Barcode",
            }),
        },
    ];

    return (
        <div className="flex w-full gap-2">
            <Button
                className="flex-1"
                variant={mode === "draw" ? "default" : "outline"}
                disabled={!isEditable}
                onClick={setDrawTool}
            >
                <DrawToolIcon />
                <ShortcutLabel
                    shortcut={DRAW_TOOL_KEY}
                    label={intl.formatMessage({
                        id: "templateEditor.sidebar.drawTool",
                        defaultMessage: "Draw",
                    })}
                />
            </Button>
            <DropdownMenu>
                <DropdownMenuTrigger asChild>
                    <Button
                        variant={mode === "draw" ? "default" : "outline"}
                        size="icon"
                        disabled={!isEditable}
                        aria-label={intl.formatMessage({
                            id: "templateEditor.sidebar.drawSubtool.title",
                            defaultMessage: "Draw Type",
                        })}
                    >
                        <ChevronDown />
                    </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end" className="w-48">
                    <DropdownMenuLabel>
                        {intl.formatMessage({
                            id: "templateEditor.sidebar.drawSubtool.title",
                            defaultMessage: "Draw Type",
                        })}
                    </DropdownMenuLabel>
                    {drawTypeOptions.map((option) => (
                        <DropdownMenuItem
                            key={option.value}
                            onClick={() => setDrawRoiType(option.value)}
                            className="flex items-center justify-between"
                        >
                            <span>{option.label}</span>
                            {drawRoiType === option.value ? (
                                <Check className="size-4" />
                            ) : null}
                        </DropdownMenuItem>
                    ))}
                    <DropdownMenuSeparator />
                    <div className="px-2 py-1 text-xs text-muted-foreground">
                        {intl.formatMessage({
                            id: "templateEditor.sidebar.drawSubtool.shortcutHint",
                            defaultMessage: "Press D in Draw mode to cycle types",
                        })}
                    </div>
                </DropdownMenuContent>
            </DropdownMenu>
        </div>
    );
};
