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
import { BacksideTemplateForm } from "@/modules/templates/actions/backside-template-form";
import { useAddTemplateBacksideMutation } from "@/modules/templates/api";
import {
    addTemplateBacksideSchema,
    type AddTemplateBacksideFormValues,
} from "@/modules/templates/schema";
import { PlusIcon } from "lucide-react";
import { useState } from "react";
import { useFormContext, useFormState } from "react-hook-form";
import { useIntl } from "react-intl";
import { toast } from "sonner";

const AddTemplateBacksideForm = ({
    onClose,
    templateId,
}: {
    onClose: () => void;
    templateId: string;
}) => {
    const intl = useIntl();
    const uploadFileMutation = useUploadFileMutation();
    const addTemplateBacksideMutation = useAddTemplateBacksideMutation();
    const { handleSubmit } = useFormContext<AddTemplateBacksideFormValues>();
    const { isSubmitting: isFormSubmitting } = useFormState();

    const onSubmit = async (values: AddTemplateBacksideFormValues) => {
        try {
            const uploadedFile = await uploadFileMutation.mutateAsync(
                values.backside.file,
            );

            await addTemplateBacksideMutation.mutateAsync({
                templateId,
                fileId: uploadedFile.id,
            });

            onClose();
            toast.success(
                intl.formatMessage({
                    id: "templates.actions.addBackside.success",
                    defaultMessage: "Backside template added successfully.",
                }),
            );
        } catch (error) {
            console.error("Error adding backside template:", error);
            toast.error(
                intl.formatMessage({
                    id: "templates.actions.addBackside.error",
                    defaultMessage: "Failed to add backside template.",
                }),
            );
        }
    };

    const isSubmitting =
        isFormSubmitting ||
        uploadFileMutation.isPending ||
        addTemplateBacksideMutation.isPending;

    return (
        <form onSubmit={handleSubmit(onSubmit)}>
            <BacksideTemplateForm
                fieldPrefix="backside"
                titleId="templates.actions.addBackside.form.title"
                titleDefaultMessage={intl.formatMessage({
                    id: "templates.actions.addBackside.form.title",
                    defaultMessage: "Upload backside template",
                })}
            />
            <DialogFooter className="gap-2">
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
                <SubmitButton disabled={isSubmitting} onSubmit={onSubmit}>
                    {isSubmitting ? (
                        <Spinner />
                    ) : (
                        intl.formatMessage({
                            id: "templates.actions.addBackside.submit",
                            defaultMessage: "Add backside",
                        })
                    )}
                </SubmitButton>
            </DialogFooter>
        </form>
    );
};

export const AddTemplateBacksideAction = ({
    templateId,
}: {
    templateId: string;
}) => {
    const [showModal, setShowModal] = useState(false);
    const intl = useIntl();

    return (
        <>
            <Button
                size="sm"
                variant="outline"
                onClick={() => setShowModal(true)}
            >
                <PlusIcon className="mr-2 h-4 w-4" />
                {intl.formatMessage({
                    id: "templates.actions.addBackside",
                    defaultMessage: "Add backside",
                })}
            </Button>
            <Dialog open={showModal} onOpenChange={setShowModal}>
                <DialogContent
                    className="sm:max-w-125"
                    onClick={(e) => e.stopPropagation()}
                    onPointerDownOutside={(e) => e.stopPropagation()}
                >
                    <DialogHeader>
                        <DialogTitle>
                            {intl.formatMessage({
                                id: "templates.actions.addBackside.title",
                                defaultMessage: "Add backside template",
                            })}
                        </DialogTitle>
                        <DialogDescription>
                            {intl.formatMessage({
                                id: "templates.actions.addBackside.description",
                                defaultMessage:
                                    "Upload a PDF file to define the backside template.",
                            })}
                        </DialogDescription>
                    </DialogHeader>
                    <Form
                        schema={addTemplateBacksideSchema}
                        defaultValues={{
                            backside: {
                                file: undefined,
                                importedConfig: undefined,
                            },
                        }}
                    >
                        <AddTemplateBacksideForm
                            onClose={() => setShowModal(false)}
                            templateId={templateId}
                        />
                    </Form>
                </DialogContent>
            </Dialog>
        </>
    );
};
