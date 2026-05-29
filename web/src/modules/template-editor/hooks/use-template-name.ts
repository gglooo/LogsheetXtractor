import type { TemplateType } from "@/modules/templates/schema";
import { defineMessage, useIntl } from "react-intl";

const backsideName = defineMessage({
    id: "templateEditor.templateName.backside",
    defaultMessage: "Backside of {name}",
});

export const useTemplateName = (template?: TemplateType) => {
    const intl = useIntl();

    if (!template) {
        return "";
    }

    return template.frontsideTemplate
        ? intl.formatMessage(backsideName, { name: template.name })
        : template.name;
};
