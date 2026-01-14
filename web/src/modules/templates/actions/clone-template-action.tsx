import { FormFileUpload } from "@/components/form/form-file-upload";
import { FormInput } from "@/components/form/form-input";
import { Button, SubmitButton } from "@/components/ui/button";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";
import { DropdownMenuItem } from "@/components/ui/dropdown-menu";
import { Form } from "@/components/ui/form";
import { Spinner } from "@/components/ui/spinner";
import { useUploadFileMutation } from "@/modules/files/api";
import { useCloneTemplateMutation } from "@/modules/templates/api";
import { pdfFileSchema } from "@/schema";
import { Copy } from "lucide-react";
import { useState } from "react";
import { useIntl } from "react-intl";
import { toast } from "sonner";
import z from "zod";

const cloneSchema = z.object({
    name: z.string().min(1).trim(),
    file: pdfFileSchema,
});

type CloneFormValues = z.infer<typeof cloneSchema>;

const CloneTemplateFormContent = ({
    onClose,
    templateId,
}: {
    onClose: () => void;
    templateId: string;
}) => {
    const intl = useIntl();

    const cloneTemplateMutation = useCloneTemplateMutation();
    const uploadFileMutation = useUploadFileMutation();

    const onSubmit = async (values: CloneFormValues) => {
        try {
            const uploadedFile = await uploadFileMutation.mutateAsync(
                values.file
            );

            await cloneTemplateMutation.mutateAsync({
                templateId,
                newName: values.name,
                fileId: uploadedFile.id,
            });

            onClose();

            toast.success(
                intl.formatMessage({
                    id: "templates.actions.clone.success",
                    defaultMessage: "Template cloned successfully!",
                })
            );
        } catch (error) {
            console.error("Error cloning template:", error);
            toast.error(
                intl.formatMessage({
                    id: "templates.actions.clone.error",
                    defaultMessage: "Failed to clone template",
                })
            );
        }
    };

    const isSubmitting =
        uploadFileMutation.isPending || cloneTemplateMutation.isPending;

    return (
        <div className="space-y-4 py-4">
            <FormInput
                name="name"
                label={intl.formatMessage({
                    id: "templates.form.nameLabel",
                    defaultMessage: "Template name",
                })}
                placeholder={intl.formatMessage({
                    id: "templates.form.namePlaceholder",
                    defaultMessage: "Enter template name",
                })}
            />

            <FormFileUpload
                name="file"
                label={intl.formatMessage({
                    id: "templates.form.fileLabel",
                    defaultMessage: "Upload PDF",
                })}
                accept=".pdf"
                validator={(file) => file.type === "application/pdf"}
            />

            <DialogFooter>
                <Button
                    variant="outline"
                    type="button"
                    disabled={isSubmitting}
                    onClick={onClose}
                >
                    {intl.formatMessage({
                        id: "common.cancel",
                        defaultMessage: "Cancel",
                    })}
                </Button>
                <SubmitButton onSubmit={onSubmit} disabled={isSubmitting}>
                    {isSubmitting ? (
                        <Spinner />
                    ) : (
                        intl.formatMessage({
                            id: "templates.actions.clone",
                            defaultMessage: "Clone template",
                        })
                    )}
                </SubmitButton>
            </DialogFooter>
        </div>
    );
};

export const CloneTemplateAction = ({ templateId }: { templateId: string }) => {
    const [showModal, setShowModal] = useState(false);
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
                    <Form
                        schema={cloneSchema}
                        defaultValues={{
                            name: "",
                            file: undefined,
                        }}
                    >
                        <CloneTemplateFormContent
                            onClose={() => setShowModal(false)}
                            templateId={templateId}
                        />
                    </Form>
                </DialogContent>
            </Dialog>
        </>
    );
};
