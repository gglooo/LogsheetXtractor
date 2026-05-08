import { FormInput } from "@/components/form/form-input";
import { Button } from "@/components/ui/button";
import { Form } from "@/components/ui/form";
import { useVerifyExtractedValueMutation } from "@/modules/logsheets/proofreading/api";
import { ExtractedValueCorrectedField } from "@/modules/logsheets/proofreading/components/extracted-value-corrected-field";
import { ExtractedValueValidationWarnings } from "@/modules/logsheets/proofreading/components/extracted-value-validation-warnings";
import { getExtractedValueDefaultFormValue } from "@/modules/logsheets/proofreading/utils";
import {
    createExtractedValueFormSchema,
    type ExtractedValueFormValues,
    type ExtractedValueType,
} from "@/modules/logsheets/schema";
import type { RoiValidationConditionType } from "@/modules/rois/validation/schema";
import { CheckIcon } from "lucide-react";
import { useFormState } from "react-hook-form";
import { useIntl } from "react-intl";
import { toast } from "sonner";

type Props = {
    extractedValue: ExtractedValueType;
    validationCondition: RoiValidationConditionType;
    onVerified?: (verifiedExtractedValue: ExtractedValueType) => void;
};

const SubmitButton = () => {
    const intl = useIntl();
    const { isSubmitting } = useFormState();

    return (
        <Button
            type="submit"
            variant="outline"
            size="sm"
            disabled={isSubmitting}
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
}: {
    extractedValue: ExtractedValueType;
}) => {
    const intl = useIntl();

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
                    />
                </div>
            </div>
            <div className="flex items-end">
                <SubmitButton />
            </div>
        </div>
    );
};

export const ExtractedValueForm = ({
    extractedValue,
    validationCondition,
    onVerified,
}: Props) => {
    const intl = useIntl();
    const verifyMutation = useVerifyExtractedValueMutation(
        extractedValue.logsheetId,
    );

    const formSchema = createExtractedValueFormSchema(extractedValue.roiType);
    const defaultCorrectedValue =
        getExtractedValueDefaultFormValue(extractedValue);

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
            onVerified?.(extractedValue);
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
                correctedValue: defaultCorrectedValue,
            }}
            onSubmit={onSubmit}
        >
            <div className="flex flex-col gap-4">
                <FormContent extractedValue={extractedValue} />
                <ExtractedValueValidationWarnings
                    warnings={extractedValue.validationWarnings}
                    validationCondition={validationCondition}
                    initialCorrectedValue={defaultCorrectedValue}
                />
            </div>
        </Form>
    );
};
