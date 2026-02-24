import { Button } from "@/components/ui/button";
import { useTemplateEditor } from "@/modules/template-editor/hooks/use-template-editor";
import { baseTemplateEditorPath } from "@/modules/template-editor/routes";
import { ArrowLeftRight } from "lucide-react";
import { useIntl } from "react-intl";

export const TemplateSideToggle = () => {
    const intl = useIntl();
    const { template } = useTemplateEditor();

    if (!template) {
        return null;
    }

    const hasBackside = !!template.backsideTemplate;
    const hasFrontside = !!template.frontsideTemplate;

    if (!hasBackside && !hasFrontside) {
        return null;
    }

    const handleToggle = () => {
        if (hasBackside && template.backsideTemplate) {
            window.location.href = `${baseTemplateEditorPath}/${template.backsideTemplate.id}`;
        } else if (hasFrontside && template.frontsideTemplate) {
            window.location.href = `${baseTemplateEditorPath}/${template.frontsideTemplate.id}`;
        }
    };

    return (
        <Button variant="outline" size="sm" onClick={handleToggle}>
            <ArrowLeftRight className="mr-2 h-4 w-4" />
            {hasFrontside
                ? intl.formatMessage({
                      id: "templateSideToggle.switchToBack",
                      defaultMessage: "Switch to back",
                  })
                : intl.formatMessage({
                      id: "templateSideToggle.switchToFront",
                      defaultMessage: "Switch to front",
                  })}
        </Button>
    );
};
