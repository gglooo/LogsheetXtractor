import { Button, SubmitButton } from "@/components/ui/button";
import { DialogFooter } from "@/components/ui/dialog";
import { Spinner } from "@/components/ui/spinner";
import { StepOne } from "@/modules/templates/actions/create-template-action/steps/step-one";
import { StepTwo } from "@/modules/templates/actions/create-template-action/steps/step-two";

import { useCreateTemplateMutation } from "@/modules/templates/api";
import type { CreateTemplateFormValues } from "@/modules/templates/schema";
import { useState } from "react";
import { useFormContext, useFormState } from "react-hook-form";
import { useIntl } from "react-intl";
import { toast } from "sonner";

export const CreateTemplateWizard = ({ onClose }: { onClose: () => void }) => {
    const intl = useIntl();
    const { trigger, handleSubmit } =
        useFormContext<CreateTemplateFormValues>();
    const { isSubmitting } = useFormState();
    const [step, setStep] = useState<1 | 2>(1);
    const createTemplateMutation = useCreateTemplateMutation();

    const onSubmit = async (values: CreateTemplateFormValues) => {
        try {
            await createTemplateMutation.mutateAsync(values);
            onClose();
            toast.success(
                intl.formatMessage({
                    id: "templates.actions.createTemplate.success",
                    defaultMessage: "Template created successfully!",
                }),
            );
        } catch (error) {
            console.log(error);
            toast.error(
                intl.formatMessage({
                    id: "templates.actions.createTemplate.error",
                    defaultMessage:
                        "An error occurred while creating the template.",
                }),
            );
        }
    };

    const handleNext = async () => {
        const isValid = await trigger(["name", "file", "importedConfig"]);
        if (isValid) {
            setStep(2);
        }
    };

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

                {step === 1 && (
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
                )}

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
        </form>
    );
};
