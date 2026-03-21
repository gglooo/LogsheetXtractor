import type { RoiValidationWarningType } from "@/modules/logsheets/schema";
import {
    defineMessage,
    defineMessages,
    useIntl,
    type MessageDescriptor,
} from "react-intl";

const WARNING_CODE_MESSAGES: Record<string, MessageDescriptor> = defineMessages(
    {
    "common.requiredNonEmpty": {
        id: "proofreading.validationWarnings.common.requiredNonEmpty",
        defaultMessage: "A value is required.",
    },
    "number.decimalScaleMax": {
        id: "proofreading.validationWarnings.number.decimalScaleMax",
        defaultMessage: "The number has too many decimal places.",
    },
    "number.integerOnly": {
        id: "proofreading.validationWarnings.number.integerOnly",
        defaultMessage: "The value must be an integer.",
    },
    "number.notInSet": {
        id: "proofreading.validationWarnings.number.notInSet",
        defaultMessage: "The value is in a forbidden set.",
    },
    "number.range": {
        id: "proofreading.validationWarnings.number.range",
        defaultMessage: "The value is out of allowed range.",
    },
    "text.allowedValues": {
        id: "proofreading.validationWarnings.text.allowedValues",
        defaultMessage: "The value is not one of the allowed values.",
    },
    "text.maxLength": {
        id: "proofreading.validationWarnings.text.maxLength",
        defaultMessage: "The value is too long.",
    },
    "text.minLength": {
        id: "proofreading.validationWarnings.text.minLength",
        defaultMessage: "The value is too short.",
    },
    "text.notRegex": {
        id: "proofreading.validationWarnings.text.notRegex",
        defaultMessage: "The value matches a forbidden pattern.",
    },
    "text.prefix": {
        id: "proofreading.validationWarnings.text.prefix",
        defaultMessage: "The value does not start with the required prefix.",
    },
    "text.regex": {
        id: "proofreading.validationWarnings.text.regex",
        defaultMessage: "The value does not match the required pattern.",
    },
    "text.suffix": {
        id: "proofreading.validationWarnings.text.suffix",
        defaultMessage: "The value does not end with the required suffix.",
    },
    "validation.error": {
        id: "proofreading.validationWarnings.validation.error",
        defaultMessage: "Validation failed.",
    },
    "validation.group.empty": {
        id: "proofreading.validationWarnings.validation.group.empty",
        defaultMessage: "Validation group has no conditions.",
    },
    "validation.group.operator": {
        id: "proofreading.validationWarnings.validation.group.operator",
        defaultMessage: "Validation group operator is invalid.",
    },
    "validation.node.invalid": {
        id: "proofreading.validationWarnings.validation.node.invalid",
        defaultMessage: "Validation configuration is invalid.",
    },
    "validation.rule.incompatible": {
        id: "proofreading.validationWarnings.validation.rule.incompatible",
        defaultMessage:
            "Validation rule is not compatible with this field type.",
    },
    "validation.rule.missing": {
        id: "proofreading.validationWarnings.validation.rule.missing",
        defaultMessage: "Validation rule type is missing.",
    },
    "validation.rule.unsupported": {
        id: "proofreading.validationWarnings.validation.rule.unsupported",
        defaultMessage: "Validation rule is not supported.",
    },
},
);

const FALLBACK_MESSAGE = defineMessage({
    id: "proofreading.validationWarnings.unknown",
    defaultMessage: "Validation warning: {code}",
});

export const useFormatValidationWarning = () => {
    const intl = useIntl();

    return (warning: RoiValidationWarningType) => {
        const descriptor =
            WARNING_CODE_MESSAGES[warning.code] ?? FALLBACK_MESSAGE;

        return intl.formatMessage(descriptor, {
            code: warning.code,
        });
    };
};
