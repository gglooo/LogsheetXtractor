import { BacksideTemplateForm } from "@/modules/templates/actions/backside-template-form";
import { useIntl } from "react-intl";

export const StepTwo = () => {
    const intl = useIntl();

    return (
        <BacksideTemplateForm
            fieldPrefix="backside"
            titleId="templates.actions.createTemplate.backside.title"
            titleDefaultMessage={intl.formatMessage({
                id: "templates.actions.createTemplate.backside.title",
                defaultMessage: "Backside Template (Optional)",
            })}
            showConfigUpload
        />
    );
};
