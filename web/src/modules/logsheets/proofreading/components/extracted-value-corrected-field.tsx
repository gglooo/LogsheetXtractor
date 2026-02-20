import { FormInput } from "@/components/form/form-input";
import { FormShadcnSelect } from "@/components/form/form-shadcn-select";
import { roiTypeSchema } from "@/modules/rois/schema";
import { useIntl } from "react-intl";
import z from "zod";

type RoiTypeEnum = z.infer<typeof roiTypeSchema>;

type ExtractedValueCorrectedFieldProps = {
    roiType: RoiTypeEnum;
};

export const ExtractedValueCorrectedField = ({
    roiType,
}: ExtractedValueCorrectedFieldProps) => {
    const intl = useIntl();

    const label = intl.formatMessage({
        id: "proofreading.extractedValue.correctedValue.label",
        defaultMessage: "Corrected value",
    });

    switch (roiType) {
        case "Number":
            return (
                <FormInput name="correctedValue" type="number" label={label} />
            );
        case "Checkbox":
            return (
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
            );
        case "Handwritten":
        case "Barcode":
        default:
            return (
                <FormInput name="correctedValue" type="text" label={label} />
            );
    }
};
