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
import { Form } from "@/components/ui/form";
import { Spinner } from "@/components/ui/spinner";
import { useUploadFileMutation } from "@/modules/files/api";
import { useCreateTemplateMutation } from "@/modules/templates/api";
import { pdfFileSchema } from "@/schema";
import { BracesIcon } from "lucide-react";
import { useState } from "react";
import { useFormState } from "react-hook-form";
import { useIntl } from "react-intl";
import { toast } from "sonner";
import z from "zod";

const createTemplateSchema = z.object({
    name: z.string().min(1).trim(),
    file: pdfFileSchema,
    importedConfig: z.instanceof(File).optional(),
});

type CreateTemplateFormValues = z.infer<typeof createTemplateSchema>;

const CreateTemplateFormContent = ({ onClose }: { onClose: () => void }) => {
    const intl = useIntl();
    const { isSubmitting } = useFormState();

    const uploadFileMutation = useUploadFileMutation();
    const createTemplateMutation = useCreateTemplateMutation();

    const onSubmit = async (values: CreateTemplateFormValues) => {
        try {
            let importedConfig: string | undefined;
            if (values.importedConfig) {
                importedConfig = await values.importedConfig.text();
            }

            const uploadedFile = await uploadFileMutation.mutateAsync(
                values.file
            );

            await createTemplateMutation.mutateAsync({
                name: values.name,
                fileId: uploadedFile.id,
                importedConfig,
            });
            onClose();
            toast.success(
                intl.formatMessage({
                    id: "templates.actions.createTemplate.success",
                    defaultMessage: "Template created successfully!",
                })
            );
        } catch (error) {
            console.log(error);
            toast.error(
                intl.formatMessage({
                    id: "templates.actions.createTemplate.error",
                    defaultMessage:
                        "An error occurred while creating the template.",
                })
            );
        }
    };

    return (
        <div className="space-y-4 py-4">
            <FormInput
                name="name"
                label={intl.formatMessage({
                    id: "templates.actions.createTemplate.form.name.label",
                    defaultMessage: "Template name",
                })}
            />

            <FormFileUpload
                name="file"
                label={intl.formatMessage({
                    id: "templates.actions.createTemplate.form.file.label",
                    defaultMessage: "Upload PDF File",
                })}
                accept=".pdf"
                validator={(file) => file.type === "application/pdf"}
            />

            <FormFileUpload
                name="importedConfig"
                label={intl.formatMessage({
                    id: "templates.actions.createTemplate.form.configFile.label",
                    defaultMessage: "Upload JSON config (optional)",
                })}
                accept=".json"
                validator={(file) => file.type === "application/json"}
                size="small"
                icon={<BracesIcon className="text-muted-foreground" />}
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
                            id: "templates.actions.createTemplate.submit",
                            defaultMessage: "Create template",
                        })
                    )}
                </SubmitButton>
            </DialogFooter>
        </div>
    );
};

export const CreateTemplateAction = () => {
    const intl = useIntl();
    const [showModal, setShowModal] = useState(false);

    return (
        <>
            <Button onClick={() => setShowModal(true)}>
                {intl.formatMessage({
                    id: "templates.actions.createTemplate",
                    defaultMessage: "Create template",
                })}
            </Button>

            <Dialog open={showModal} onOpenChange={setShowModal}>
                <DialogContent>
                    <DialogHeader>
                        <DialogTitle>
                            {intl.formatMessage({
                                id: "templates.actions.createTemplate",
                                defaultMessage: "Create template",
                            })}
                        </DialogTitle>
                        <DialogDescription>
                            {intl.formatMessage({
                                id: "templates.actions.createTemplate.description",
                                defaultMessage:
                                    "Create a new template by uploading a PDF file.",
                            })}
                        </DialogDescription>
                    </DialogHeader>

                    <Form
                        schema={createTemplateSchema}
                        defaultValues={{
                            name: "",
                            file: undefined,
                            importedConfig: undefined,
                        }}
                    >
                        <CreateTemplateFormContent
                            onClose={() => {
                                setShowModal(false);
                            }}
                        />
                    </Form>
                </DialogContent>
            </Dialog>
        </>
    );
};
