import { Button, SubmitButton } from "@/components/ui/button";
import { DialogFooter } from "@/components/ui/dialog";
import { Spinner } from "@/components/ui/spinner";
import { useUploadFileMutation } from "@/modules/files/api";
import { StepOne } from "@/modules/templates/actions/clone-template-action/steps/step-one";
import { StepTwo } from "@/modules/templates/actions/clone-template-action/steps/step-two";
import { useCloneTemplateMutation } from "@/modules/templates/api";
import type { CloneTemplateFormValues } from "@/modules/templates/schema";
import { useState } from "react";
import { useFormContext, useFormState } from "react-hook-form";
import { useIntl } from "react-intl";
import { toast } from "sonner";

export const CloneTemplateWizard = ({
    onClose,
    templateId,
}: {
    onClose: () => void;
    templateId: string;
}) => {
    const intl = useIntl();
    const { trigger, handleSubmit } = useFormContext<CloneTemplateFormValues>();
    const { isSubmitting: isFormSubmitting } = useFormState();
    const [step, setStep] = useState<1 | 2>(1);

    const cloneTemplateMutation = useCloneTemplateMutation();
    const uploadFileMutation = useUploadFileMutation();

    const onSubmit = async (values: CloneTemplateFormValues) => {
        try {
            const uploadedFile = await uploadFileMutation.mutateAsync(
                values.file,
            );

            let backside;
            if (values.backside?.name && values.backside?.file) {
                const backsideUploadedFile =
                    await uploadFileMutation.mutateAsync(values.backside.file);
                backside = {
                    name: values.backside.name,
                    fileId: backsideUploadedFile.id,
                };
            }

            await cloneTemplateMutation.mutateAsync({
                templateId,
                newName: values.name,
                fileId: uploadedFile.id,
                backside,
            });

            onClose();

            toast.success(
                intl.formatMessage({
                    id: "templates.actions.clone.success",
                    defaultMessage: "Template cloned successfully!",
                }),
            );
        } catch (error) {
            console.error("Error cloning template:", error);
            toast.error(
                intl.formatMessage({
                    id: "templates.actions.clone.error",
                    defaultMessage: "Failed to clone template",
                }),
            );
        }
    };

    const handleNext = async () => {
        const isValid = await trigger(["name", "file"]);
        if (isValid) {
            setStep(2);
        }
    };

    const isSubmitting =
        isFormSubmitting ||
        uploadFileMutation.isPending ||
        cloneTemplateMutation.isPending;

    return (
        <form onSubmit={handleSubmit(onSubmit)}>
            {step === 1 && <StepOne />}
            {step === 2 && <StepTwo />}

            <DialogFooter className="gap-2">
                {step === 2 && (
                    <Button
                        variant="secondary"
                        type="button"
                        onClick={() => setStep(1)}
                        className="mr-auto"
                        disabled={isSubmitting}
                    >
                        {intl.formatMessage({
                            id: "common.back",
                            defaultMessage: "Back",
                        })}
                    </Button>
                )}

                {step === 1 ? (
                    <>
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
                        <Button
                            variant="outline"
                            type="button"
                            onClick={handleNext}
                            disabled={isSubmitting}
                        >
                            {intl.formatMessage({
                                id: "templates.actions.createTemplate.defineBackside",
                                defaultMessage: "Define backside",
                            })}
                        </Button>
                    </>
                ) : null}

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
        </form>
    );
};
