import { Button } from "@/components/ui/button";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";
import { DropdownMenuItem } from "@/components/ui/dropdown-menu";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Spinner } from "@/components/ui/spinner";
import { useUploadFileMutation } from "@/modules/files/api";
import { useCloneTemplateMutation } from "@/modules/templates/api";
import { Copy, Upload } from "lucide-react";
import { useState } from "react";
import { useIntl } from "react-intl";

export const CloneTemplateAction = ({ templateId }: { templateId: string }) => {
    const [showModal, setShowModal] = useState(false);
    const [newTemplateName, setNewTemplateName] = useState("");
    const [newTemplateFile, setNewTemplateFile] = useState<File | null>(null);

    const cloneTemplateMutation = useCloneTemplateMutation();
    const uploadFileMutation = useUploadFileMutation();

    const handleClone = async () => {
        if (!newTemplateFile) return;

        try {
            const uploadedFile = await uploadFileMutation.mutateAsync(
                newTemplateFile
            );
            await cloneTemplateMutation.mutateAsync({
                templateId,
                newName: newTemplateName,
                fileId: uploadedFile.id,
            });

            setShowModal(false);
            setNewTemplateName("");
            setNewTemplateFile(null);
        } catch (error) {
            console.error("Error cloning template:", error);
        }
    };

    const intl = useIntl();

    return (
        <>
            <DropdownMenuItem
                onSelect={(e) => {
                    e.preventDefault();
                    setShowModal(true);
                }}
            >
                <Copy className="mr-2 h-4 w-4" />
                {intl.formatMessage({
                    id: "templates.actions.clone",
                    defaultMessage: "Clone Template",
                })}
            </DropdownMenuItem>
            <Dialog open={showModal} onOpenChange={setShowModal}>
                <DialogContent className="sm:max-w-125">
                    <DialogHeader>
                        <DialogTitle>
                            {intl.formatMessage({
                                id: "templates.actions.clone.title",
                                defaultMessage: "Clone a template",
                            })}
                        </DialogTitle>
                        <DialogDescription>
                            {intl.formatMessage({
                                id: "templates.actions.clone.description",
                                defaultMessage:
                                    "Upload a new PDF to create a cloned template. All ROIs from the original template will be copied to the new one.",
                            })}
                        </DialogDescription>
                    </DialogHeader>

                    <div className="space-y-4 py-4">
                        <div className="space-y-2">
                            <Label htmlFor="name">
                                {intl.formatMessage({
                                    id: "templates.form.nameLabel",
                                    defaultMessage: "Template name",
                                })}
                            </Label>
                            <Input
                                id="name"
                                placeholder={intl.formatMessage({
                                    id: "templates.form.namePlaceholder",
                                    defaultMessage: "Enter template name",
                                })}
                                value={newTemplateName}
                                onChange={(e) =>
                                    setNewTemplateName(e.target.value)
                                }
                            />
                        </div>

                        <div className="space-y-2">
                            <Label>
                                {intl.formatMessage({
                                    id: "templates.form.pdfLabel",
                                    defaultMessage: "Template PDF",
                                })}
                            </Label>
                            <div className="border-2 border-dashed border-border rounded-lg p-8 text-center hover:border-primary/50 transition-colors cursor-pointer">
                                <input
                                    type="file"
                                    accept=".pdf"
                                    className="hidden"
                                    id="pdf-upload"
                                    onChange={(e) =>
                                        setNewTemplateFile(
                                            e.target.files?.[0] || null
                                        )
                                    }
                                />
                                <label
                                    htmlFor="pdf-upload"
                                    className="cursor-pointer"
                                >
                                    <Upload className="h-8 w-8 mx-auto mb-2 text-muted-foreground" />
                                    <p className="text-sm font-medium">
                                        {newTemplateFile
                                            ? newTemplateFile.name
                                            : intl.formatMessage({
                                                  id: "templates.upload.prompt",
                                                  defaultMessage:
                                                      "Click to upload PDF",
                                              })}
                                    </p>
                                    <p className="text-xs text-muted-foreground mt-1">
                                        {intl.formatMessage({
                                            id: "templates.upload.dragDrop",
                                            defaultMessage: "or drag and drop",
                                        })}
                                    </p>
                                </label>
                            </div>
                        </div>
                    </div>

                    <DialogFooter>
                        <Button
                            variant="outline"
                            disabled={cloneTemplateMutation.isPending}
                            onClick={() => setShowModal(false)}
                        >
                            {intl.formatMessage({
                                id: "common.cancel",
                                defaultMessage: "Cancel",
                            })}
                        </Button>
                        <Button
                            onClick={handleClone}
                            disabled={!newTemplateName || !newTemplateFile}
                        >
                            {cloneTemplateMutation.isPending ? (
                                <Spinner />
                            ) : (
                                intl.formatMessage({
                                    id: "templates.actions.clone",
                                    defaultMessage: "Clone template",
                                })
                            )}
                        </Button>
                    </DialogFooter>
                </DialogContent>
            </Dialog>
        </>
    );
};
