import { Button } from "@/components/ui/button";

export const EditorNavbar = () => {
    return (
        <header className="sticky z-50 border-b border-border bg-background/95 backdrop-blur supports-backdrop-filter:bg-background/60">
            <div className="flex justify-between">
                <div className="p-4 text-lg font-bold">Template Editor</div>
                <div className="flex items-center gap-2 p-4">
                    <Button size="sm" variant="outline">
                        Cancel
                    </Button>
                    <Button size="sm">Save Changes</Button>
                </div>
            </div>
        </header>
    );
};
