import { FormFileUpload } from "@/components/form/form-file-upload";
import { FormInput } from "@/components/form/form-input";
import type { CreateTemplateFormValues } from "@/modules/templates/schema";
import { BracesIcon } from "lucide-react";
import { useFormContext } from "react-hook-form";
import { useIntl } from "react-intl";

export const StepTwo = () => {
    const intl = useIntl();
    const { setValue } = useFormContext<CreateTemplateFormValues>();

    const onFileChange = (file: File | File[] | null) => {
        if (!file || Array.isArray(file)) {
            return;
        }
        setValue("backside.name", file.name.replace(/\.[^/.]+$/, ""));
    };

    return (
        <div className="space-y-4 py-4">
            <h3 className="font-medium">
                {intl.formatMessage({
                    id: "templates.actions.createTemplate.backside.title",
                    defaultMessage: "Backside Template (Optional)",
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
                onChange={onFileChange}
            />

            <FormInput
                name="backside.name"
                label={intl.formatMessage({
                    id: "templates.actions.createTemplate.form.name.label",
                    defaultMessage: "Template name",
                })}
            />

            <FormFileUpload
                name="backside.importedConfig"
                label={intl.formatMessage({
                    id: "templates.actions.createTemplate.form.configFile.label",
                    defaultMessage: "Upload JSON config (optional)",
                })}
                accept=".json"
                validator={(file) => file.type === "application/json"}
                size="small"
                icon={<BracesIcon className="text-muted-foreground" />}
            />
        </div>
    );
};
