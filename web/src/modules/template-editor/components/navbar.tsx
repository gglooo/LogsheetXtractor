import { Button } from "@/components/ui/button";
import { Spinner } from "@/components/ui/spinner";
import { useSetRoisMutation } from "@/modules/rois/api";
import { useTemplateEditor } from "@/modules/template-editor/hooks/use-template-editor";
import { toast } from "sonner";

export const EditorNavbar = () => {
    const { rois, template } = useTemplateEditor();
    const setRoisMutation = useSetRoisMutation(template?.id);

    const isSavingChanges = setRoisMutation.isPending;

    const handleSaveChanges = async () => {
        if (!template) {
            toast.error("No template loaded.");
            return;
        }

        try {
            await setRoisMutation.mutateAsync({
                rois,
            });
            toast.success("ROIs saved successfully.");
        } catch (error) {
            console.error("Error saving ROIs:", error);
            toast.error("Failed to save ROIs.");
        }
    };

    return (
        <header className="sticky z-50 border-b border-border bg-background/95 backdrop-blur supports-backdrop-filter:bg-background/60">
            <div className="flex justify-between">
                <div className="p-4 text-lg font-bold">Template Editor</div>
                <div className="flex items-center gap-2 p-4">
                    <Button size="sm" variant="outline">
                        Cancel
                    </Button>
                    <Button size="sm" onClick={handleSaveChanges}>
                        {isSavingChanges ? <Spinner /> : null}Save Changes
                    </Button>
                </div>
            </div>
        </header>
    );
};
