import { FormFileUpload } from "@/components/form/form-file-upload";
import { FormInput } from "@/components/form/form-input";
import type { CloneTemplateFormValues } from "@/modules/templates/schema";
import { useFormContext } from "react-hook-form";
import { useIntl } from "react-intl";

export const StepOne = () => {
    const intl = useIntl();
    const { setValue } = useFormContext<CloneTemplateFormValues>();

    const onFileChange = (file: File | File[] | null) => {
        if (!file || Array.isArray(file)) {
            return;
        }
        setValue("name", file.name.replace(/\.[^/.]+$/, ""));
    };

    return (
        <div className="space-y-4 py-4">
            <FormFileUpload
                name="file"
                label={intl.formatMessage({
                    id: "templates.form.fileLabel",
                    defaultMessage: "Upload PDF File",
                })}
                accept=".pdf"
                validator={(file) => file.type === "application/pdf"}
                onChange={onFileChange}
            />

            <FormInput
                name="name"
                label={intl.formatMessage({
                    id: "templates.form.nameLabel",
                    defaultMessage: "Template name",
                })}
                placeholder={intl.formatMessage({
                    id: "templates.form.namePlaceholder",
                    defaultMessage: "Enter template name",
                })}
            />
        </div>
    );
};
