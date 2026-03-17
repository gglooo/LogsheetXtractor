import { FormInput } from "@/components/form/form-input";
import { Button } from "@/components/ui/button";
import { Form } from "@/components/ui/form";
import { useVerifyExtractedValueMutation } from "@/modules/logsheets/proofreading/api";
import { ExtractedValueCorrectedField } from "@/modules/logsheets/proofreading/components/extracted-value-corrected-field";
import { getExtractedValueDefaultFormValue } from "@/modules/logsheets/proofreading/utils";
import {
    createExtractedValueFormSchema,
    type ExtractedValueFormValues,
    type ExtractedValueType,
} from "@/modules/logsheets/schema";
import { CheckIcon } from "lucide-react";
import { useFormContext, useFormState } from "react-hook-form";
import { useIntl } from "react-intl";
import { toast } from "sonner";

type Props = {
    extractedValue: ExtractedValueType;
};

const SubmitButton = ({ onSubmit }: { onSubmit: () => void }) => {
    const intl = useIntl();
    const { isSubmitting } = useFormState();

    return (
        <Button
            type="button"
            variant="outline"
            size="sm"
            disabled={isSubmitting}
            onClick={onSubmit}
            className="h-9 w-9 p-0"
        >
            <CheckIcon className="h-4 w-4" />
            <span className="sr-only">
                {intl.formatMessage({
                    id: "proofreading.extractedValue.verify",
                    defaultMessage: "Verify",
                })}
            </span>
        </Button>
    );
};

const FormContent = ({
    extractedValue,
    onSubmitCallback,
}: {
    extractedValue: ExtractedValueType;
    onSubmitCallback: (values: ExtractedValueFormValues) => Promise<void>;
}) => {
    const intl = useIntl();
    const { handleSubmit } = useFormContext<ExtractedValueFormValues>();

    const onSubmit = () => {
        handleSubmit(onSubmitCallback)();
    };

    return (
        <div className="flex gap-2">
            <div className="flex-1 flex gap-2">
                <div className="flex-1 flex flex-col gap-2">
                    <FormInput
                        label={intl.formatMessage({
                            id: "proofreading.extractedValueForm.originalValue.label",
                            defaultMessage: "Original",
                        })}
                        value={extractedValue.value}
                        name="value"
                        readOnly
                        className="h-9 text-sm"
                    />
                </div>
                <div className="flex-1">
                    <ExtractedValueCorrectedField
                        roiType={extractedValue.roiType}
                        ocrValue={extractedValue.value}
                    />
                </div>
            </div>
            <div className="flex items-end">
                <SubmitButton onSubmit={onSubmit} />
            </div>
        </div>
    );
};

export const ExtractedValueForm = ({ extractedValue }: Props) => {
    const intl = useIntl();
    const verifyMutation = useVerifyExtractedValueMutation(
        extractedValue.logsheetId,
    );

    const formSchema = createExtractedValueFormSchema(extractedValue.roiType);

    const onSubmit = async (values: ExtractedValueFormValues) => {
        try {
            await verifyMutation.mutateAsync({
                extractedValueId: extractedValue.id,
                correctedValue: values.correctedValue?.toString() ?? undefined,
            });
            toast.success(
                intl.formatMessage({
                    id: "proofreading.extractedValue.verify.success",
                    defaultMessage: "Value verified successfully",
                }),
            );
        } catch {
            toast.error(
                intl.formatMessage({
                    id: "proofreading.extractedValue.verify.error",
                    defaultMessage: "Failed to verify value",
                }),
            );
        }
    };

    return (
        <Form
            schema={formSchema}
            defaultValues={{
                ...extractedValue,
                correctedValue:
                    getExtractedValueDefaultFormValue(extractedValue),
            }}
        >
            <FormContent
                extractedValue={extractedValue}
                onSubmitCallback={onSubmit}
            />
        </Form>
    );
};
