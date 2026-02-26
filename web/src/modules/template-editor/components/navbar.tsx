import { NavbarContainer } from "@/components/navbar-container";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Spinner } from "@/components/ui/spinner";
import { useSetRoisMutation } from "@/modules/rois/api";
import { CancelDialog } from "@/modules/template-editor/components/cancel-dialog";
import { TemplateSideToggle } from "@/modules/template-editor/components/template-side-toggle";
import { useTemplateEditor } from "@/modules/template-editor/hooks/use-template-editor";
import { ArrowLeft } from "lucide-react";
import { useState } from "react";
import { useIntl } from "react-intl";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";

export const EditorNavbar = () => {
    const navigate = useNavigate();

    const intl = useIntl();

    const [isCancelDialogOpen, setCancelDialogOpen] = useState(false);

    const { rois, template, duplicateRoiNames, isDirty, markAsSaved } =
        useTemplateEditor();
    const setRoisMutation = useSetRoisMutation(template?.id);

    const isSavingChanges = setRoisMutation.isPending;

    const handleSaveChanges = async () => {
        if (!template) {
            toast.error("No template loaded.");
            return;
        }

        if (duplicateRoiNames.size > 0) {
            toast.error(
                intl.formatMessage({
                    id: "template-editor.navbar.save-rois.duplicate-roi-message",
                    defaultMessage:
                        "There are duplicate ROI variable names, please name then uniquely.",
                }),
            );
            return;
        }

        try {
            await setRoisMutation.mutateAsync({
                rois,
            });
            markAsSaved();
            toast.success("ROIs saved successfully.");
        } catch (error) {
            console.error("Error saving ROIs:", error);
            toast.error("Failed to save ROIs.");
        }
    };

    const handleCancel = () => {
        if (isDirty && !isCancelDialogOpen && template?.isEditable) {
            setCancelDialogOpen(true);
            return;
        }
        navigate(-1);
    };

    return (
        <NavbarContainer
            AsideContent={
                isDirty && template?.isEditable ? (
                    <CancelDialog
                        open={isCancelDialogOpen}
                        onCancel={handleCancel}
                        onClose={() => setCancelDialogOpen(false)}
                    />
                ) : null
            }
        >
            <div className="flex gap-4 items-center">
                <Button
                    variant="ghost"
                    onClick={handleCancel}
                    disabled={isSavingChanges}
                >
                    <ArrowLeft />
                </Button>
                <div className="text-lg font-bold">
                    {intl.formatMessage({
                        id: "template-editor.navbar.title",
                        defaultMessage: "Template Editor",
                    })}
                </div>
                {!template?.isEditable && (
                    <Badge variant="default">
                        {intl.formatMessage({
                            id: "template.editor.read-only",
                            defaultMessage: "Read only",
                        })}
                    </Badge>
                )}
            </div>
            <div className="flex items-center gap-2 p-4">
                <TemplateSideToggle />
                <Button size="sm" variant="outline" onClick={handleCancel}>
                    {intl.formatMessage({
                        id: "template-editor.navbar.cancel",
                        defaultMessage: "Cancel",
                    })}
                </Button>
                <Button
                    size="sm"
                    onClick={handleSaveChanges}
                    disabled={!template?.isEditable || isSavingChanges}
                >
                    {isSavingChanges ? <Spinner /> : null}
                    {intl.formatMessage({
                        id: "template-editor.navbar.save-changes",
                        defaultMessage: "Save Changes",
                    })}
                </Button>
            </div>
        </NavbarContainer>
    );
};
