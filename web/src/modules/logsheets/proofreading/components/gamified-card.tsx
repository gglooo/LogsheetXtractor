import { FormInput } from "@/components/form/form-input";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Form } from "@/components/ui/form";
import { Spinner } from "@/components/ui/spinner";
import { useVerifyExtractedValueMutation } from "@/modules/logsheets/proofreading/api";
import { ExtractedValueCorrectedField } from "@/modules/logsheets/proofreading/components/extracted-value-corrected-field";
import { ExtractedValueImage } from "@/modules/logsheets/proofreading/components/extracted-value-image";
import { ExtractedValueValidationWarnings } from "@/modules/logsheets/proofreading/components/extracted-value-validation-warnings";
import { useExtractedValueValidationCondition } from "@/modules/logsheets/proofreading/hooks/use-extracted-value-validation-condition";
import { getExtractedValueDefaultFormValue } from "@/modules/logsheets/proofreading/utils";
import {
    createExtractedValueFormSchema,
    type ExtractedValueFormValues,
    type ExtractedValueType,
} from "@/modules/logsheets/schema";
import { CheckIcon } from "lucide-react";
import { useFormState } from "react-hook-form";
import { useIntl } from "react-intl";
import { toast } from "sonner";

type GamifiedCardProps = {
    extractedValue: ExtractedValueType;
    isFetching?: boolean;
    onNext: () => void;
    onSkip?: () => void;
};

const SubmitButton = ({
    onSkip,
    isFetching,
}: {
    onSkip?: () => void;
    isFetching?: boolean;
}) => {
    const intl = useIntl();
    const { isSubmitting } = useFormState();

    const isDisabled = isSubmitting || isFetching;

    return (
        <div className="flex gap-2 w-full">
            {onSkip && (
                <Button
                    type="button"
                    variant="outline"
                    className="flex-1"
                    disabled={isDisabled}
                    onClick={onSkip}
                >
                    {intl.formatMessage({
                        id: "gamified.card.skip",
                        defaultMessage: "Skip",
                    })}
                </Button>
            )}
            <Button
                type="submit"
                className="flex-1 gap-2"
                disabled={isDisabled}
            >
                {isDisabled ? <Spinner /> : <CheckIcon className="h-4 w-4" />}
                {intl.formatMessage({
                    id: "gamified.card.submit",
                    defaultMessage: "Submit",
                })}
            </Button>
        </div>
    );
};

const GamifiedCardForm = ({
    extractedValue,
    isFetching,
    onSkip,
}: {
    extractedValue: ExtractedValueType;
    isFetching?: boolean;
    onSkip?: () => void;
}) => {
    const intl = useIntl();

    return (
        <div className="flex flex-col gap-4">
            <FormInput
                label={intl.formatMessage({
                    id: "proofreading.extractedValue.originalValue.label",
                    defaultMessage: "Extracted value",
                })}
                value={extractedValue.value}
                name="value"
                readOnly
            />
            <ExtractedValueCorrectedField roiType={extractedValue.roiType} />
            <SubmitButton
                onSkip={onSkip}
                isFetching={isFetching}
            />
        </div>
    );
};

export const GamifiedCard = ({
    extractedValue,
    isFetching,
    onNext,
    onSkip,
}: GamifiedCardProps) => {
    const intl = useIntl();
    const verifyMutation = useVerifyExtractedValueMutation(
        extractedValue.logsheetId,
    );
    const validationCondition = useExtractedValueValidationCondition(
        extractedValue.logsheetId,
        extractedValue.roiId,
    );
    const defaultCorrectedValue = getExtractedValueDefaultFormValue(
        extractedValue,
    );

    const formSchema = createExtractedValueFormSchema(extractedValue.roiType);

    const handleSubmit = async (values: ExtractedValueFormValues) => {
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
            onNext();
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
        <Card className="w-full max-w-xl mx-auto shadow-md">
            <CardContent className="flex flex-col gap-5 p-6">
                <span className="text-xs font-medium text-muted-foreground uppercase tracking-wide">
                    {extractedValue.variableName}
                </span>
                <div
                    className={
                        isFetching
                            ? "opacity-50 pointer-events-none transition-opacity"
                            : "transition-opacity"
                    }
                >
                    <ExtractedValueImage id={extractedValue.id} size="lg" />
                </div>
                <Form
                    key={extractedValue.id}
                    schema={formSchema}
                    defaultValues={{
                        ...extractedValue,
                        correctedValue: defaultCorrectedValue,
                    }}
                    onSubmit={handleSubmit}
                >
                    <div className="flex flex-col gap-2">
                        <GamifiedCardForm
                            extractedValue={extractedValue}
                            onSkip={onSkip}
                            isFetching={isFetching}
                        />
                        <ExtractedValueValidationWarnings
                            warnings={extractedValue.validationWarnings}
                            validationCondition={validationCondition}
                            initialCorrectedValue={defaultCorrectedValue}
                        />
                    </div>
                </Form>
            </CardContent>
        </Card>
    );
};
