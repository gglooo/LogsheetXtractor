import { Button } from "@/components/ui/button";
import { Kbd } from "@/components/ui/kbd";
import {
    Tooltip,
    TooltipContent,
    TooltipTrigger,
} from "@/components/ui/tooltip";
import {
    useShortcutRegistry,
    type ShortcutKey,
} from "@/modules/template-editor/hooks/shortcuts/types";
import { InfoIcon } from "lucide-react";
import { useIntl } from "react-intl";

export const ShortcutBadge = ({
    keyCombination,
    description,
}: {
    keyCombination: ShortcutKey;
    description: string;
}) => {
    return (
        <div className="flex items-center justify-between gap-4 py-1">
            <span className="text-xs text-background">{description}</span>
            <div className="flex items-center gap-1">
                <Kbd>{keyCombination}</Kbd>
            </div>
        </div>
    );
};

export const ShortcutTooltip = () => {
    const intl = useIntl();

    const shortcutRegistry = useShortcutRegistry();
    return (
        <Tooltip>
            <TooltipTrigger asChild>
                <Button
                    variant="ghost"
                    size="sm"
                    className="w-full text-muted-foreground hover:text-foreground"
                >
                    <InfoIcon className="mr-2 size-3.5" />
                    <span className="text-xs">
                        {intl.formatMessage({
                            id: "template-editor.sidebar.shortcuts.buttonLabel",
                            defaultMessage: "Keyboard shortcuts",
                        })}
                    </span>
                </Button>
            </TooltipTrigger>
            <TooltipContent
                side="right"
                className="w-64 p-3 shadow-xl border-border"
            >
                <div className="space-y-1">
                    <p className="text-[11px] font-semibold uppercase tracking-wider text-muted-foreground mb-2">
                        {intl.formatMessage({
                            id: "template-editor.sidebar.shortcuts.title",
                            defaultMessage: "Keyboard shortcuts",
                        })}
                    </p>
                    <div className="divide-y divide-border/40">
                        {shortcutRegistry.map((shortcut) =>
                            shortcut.keys.map((keyCombination) => (
                                <ShortcutBadge
                                    key={`${shortcut.actionKey}-${keyCombination}`}
                                    description={shortcut.description}
                                    keyCombination={keyCombination}
                                />
                            )),
                        )}
                    </div>
                </div>
            </TooltipContent>
        </Tooltip>
    );
};
