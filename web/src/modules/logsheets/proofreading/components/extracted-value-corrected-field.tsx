import { FormInput } from "@/components/form/form-input";
import { FormShadcnSelect } from "@/components/form/form-shadcn-select";
import { cn } from "@/lib/utils";
import { roiTypeSchema } from "@/modules/rois/schema";
import { useWatch } from "react-hook-form";
import { useIntl } from "react-intl";
import z from "zod";

type RoiTypeEnum = z.infer<typeof roiTypeSchema>;

type ExtractedValueCorrectedFieldProps = {
    roiType: RoiTypeEnum;
    ocrValue: string;
};

const isNumericValue = (value: string) => {
    const normalized = value.trim().replace(",", ".");
    if (normalized.length === 0) {
        return true;
    }

    return /^[-+]?(?:\d+\.?\d*|\.\d+)$/.test(normalized);
};

const isBooleanValue = (value: string) => {
    const normalized = value.trim().toLowerCase();
    if (normalized.length === 0) {
        return true;
    }

    return normalized === "true" || normalized === "false";
};

const isOcrValueApplicableForType = (roiType: RoiTypeEnum, value: string) => {
    switch (roiType) {
        case "Number":
            return isNumericValue(value);
        case "Checkbox":
            return isBooleanValue(value);
        case "Handwritten":
        case "Barcode":
        default:
            return true;
    }
};

export const ExtractedValueCorrectedField = ({
    roiType,
    ocrValue,
}: ExtractedValueCorrectedFieldProps) => {
    const intl = useIntl();
    const isApplicable = isOcrValueApplicableForType(roiType, ocrValue);
    const correctedValue = useWatch({ name: "correctedValue" });

    const label = intl.formatMessage({
        id: "proofreading.extractedValue.correctedValue.label",
        defaultMessage: "Corrected value",
    });

    const renderOcrTypeWarning = () => {
        const shouldShowWarning =
            !isApplicable &&
            (correctedValue === null || correctedValue === undefined);

        if (!shouldShowWarning) {
            return null;
        }

        return (
            <p className={cn("text-destructive text-sm")}>
                {intl.formatMessage({
                    id: "proofreading.extractedValue.ocrValueTypeMismatch",
                    defaultMessage:
                        "OCR value does not match the expected data type and is likely incorrect.",
                })}
            </p>
        );
    };

    switch (roiType) {
        case "Number":
            return (
                <div className="space-y-2">
                    <FormInput
                        name="correctedValue"
                        type="number"
                        label={label}
                    />
                    {renderOcrTypeWarning()}
                </div>
            );
        case "Checkbox":
            return (
                <div className="space-y-2">
                    <FormShadcnSelect
                        name="correctedValue"
                        label={label}
                        options={[
                            {
                                label: intl.formatMessage({
                                    id: "common.true",
                                    defaultMessage: "True",
                                }),
                                value: "True",
                            },
                            {
                                label: intl.formatMessage({
                                    id: "common.false",
                                    defaultMessage: "False",
                                }),
                                value: "False",
                            },
                        ]}
                    />
                    {renderOcrTypeWarning()}
                </div>
            );
        case "Handwritten":
        case "Barcode":
        default:
            return (
                <div className="space-y-2">
                    <FormInput
                        name="correctedValue"
                        type="text"
                        label={label}
                    />
                    {renderOcrTypeWarning()}
                </div>
            );
    }
};
