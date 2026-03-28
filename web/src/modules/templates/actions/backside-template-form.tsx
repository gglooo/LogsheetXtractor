import { FormFileUpload } from "@/components/form/form-file-upload";
import { BracesIcon } from "lucide-react";
import { useFormContext } from "react-hook-form";
import { useIntl } from "react-intl";

type BacksideTemplateFormProps = {
    fieldPrefix: string;
    titleId: string;
    titleDefaultMessage: string;
    uploadOnly?: boolean;
    showConfigUpload?: boolean;
};

export const BacksideTemplateForm = ({
    fieldPrefix,
    titleId,
    titleDefaultMessage,
    uploadOnly = false,
    showConfigUpload = false,
}: BacksideTemplateFormProps) => {
    const intl = useIntl();
    const { setValue } = useFormContext();

    const onFileChange = (file: File | File[] | null) => {
        if (!file || Array.isArray(file)) {
            return;
        }

        setValue(`${fieldPrefix}.name`, file.name.replace(/\.[^/.]+$/, ""), {
            shouldValidate: true,
        });
    };

    return (
        <div className="space-y-4 py-4">
            <h3 className="font-medium">
                {intl.formatMessage({
                    id: titleId,
                    defaultMessage: titleDefaultMessage,
                })}
            </h3>

            <FormFileUpload
                name={`${fieldPrefix}.file`}
                label={intl.formatMessage({
                    id: "templates.actions.createTemplate.form.file.label",
                    defaultMessage: "Upload PDF File",
                })}
                accept=".pdf"
                validator={(file) => file.type === "application/pdf"}
                onChange={onFileChange}
            />

            {showConfigUpload && !uploadOnly && (
                <FormFileUpload
                    name={`${fieldPrefix}.importedConfig`}
                    label={intl.formatMessage({
                        id: "templates.actions.createTemplate.form.configFile.label",
                        defaultMessage: "Upload JSON config (optional)",
                    })}
                    accept=".json"
                    validator={(file) => file.type === "application/json"}
                    size="small"
                    icon={<BracesIcon className="text-muted-foreground" />}
                />
            )}
        </div>
    );
};
