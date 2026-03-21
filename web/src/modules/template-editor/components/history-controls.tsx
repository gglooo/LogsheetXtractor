import { Button } from "@/components/ui/button";
import { useTemplateEditor } from "@/modules/template-editor/hooks/use-template-editor";
import { Redo, Undo } from "lucide-react";

export const HistoryControls = () => {
    const { undo, redo, canUndo, canRedo } = useTemplateEditor();

    return (
        <div>
            <Button
                variant="ghost"
                size="icon"
                onClick={undo}
                disabled={!canUndo}
                title="Undo"
            >
                <Undo />
            </Button>
            <Button
                variant="ghost"
                size="icon"
                onClick={redo}
                disabled={!canRedo}
                title="Redo"
            >
                <Redo />
            </Button>
        </div>
    );
};
