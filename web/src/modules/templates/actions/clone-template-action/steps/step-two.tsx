import { FormFileUpload } from "@/components/form/form-file-upload";
import { useIntl } from "react-intl";

export const StepTwo = () => {
    const intl = useIntl();

    return (
        <div className="space-y-4 py-4">
            <h3 className="font-medium">
                {intl.formatMessage({
                    id: "templates.actions.cloneTemplate.backside.title",
                    defaultMessage: "Backside template (Optional)",
                })}
            </h3>

            <FormFileUpload
                name="backside.file"
                label={intl.formatMessage({
                    id: "templates.actions.createTemplate.form.file.label",
                    defaultMessage: "Upload PDF File",
                })}
                accept=".pdf"
                validator={(file) => file.type === "application/pdf"}
            />
        </div>
    );
};
