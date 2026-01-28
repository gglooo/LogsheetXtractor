import { Button } from "@/components/ui/button";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { baseTemplateEditorPath } from "@/modules/template-editor/routes";
import type { TemplateListItemType } from "@/modules/templates/schema";
import { EditIcon, FileTextIcon, SendToBackIcon } from "lucide-react";
import { useIntl } from "react-intl";
import { useNavigate } from "react-router-dom";

export const TemplateEditButton = ({
    template,
}: {
    template: TemplateListItemType;
}) => {
    const intl = useIntl();
    const navigate = useNavigate();

    const handleEditTemplate = (isBackside: boolean) => {
        const templateId = isBackside
            ? template.backsideTemplateId
            : template.id;

        navigate(`${baseTemplateEditorPath}/${templateId}`);
    };
    const EditButton = (
        <Button
            variant="outline"
            size="sm"
            className="flex-1 gap-2"
            onClick={() => handleEditTemplate(false)}
        >
            <EditIcon />
            {intl.formatMessage({
                id: "templates.actions.edit",
                defaultMessage: "Edit",
            })}
        </Button>
    );

    if (!template.backsideTemplateId) {
        return EditButton;
    }

    return (
        <DropdownMenu>
            <DropdownMenuContent align="end">
                <DropdownMenuItem onClick={() => handleEditTemplate(true)}>
                    <FileTextIcon />
                    {intl.formatMessage({
                        id: "templates.actions.edit-front",
                        defaultMessage: "Frontside",
                    })}
                </DropdownMenuItem>
                <DropdownMenuItem onClick={() => handleEditTemplate(false)}>
                    <SendToBackIcon />
                    {intl.formatMessage({
                        id: "templates.actions.editBackside",
                        defaultMessage: "Backside",
                    })}
                </DropdownMenuItem>
            </DropdownMenuContent>
            <DropdownMenuTrigger asChild>{EditButton}</DropdownMenuTrigger>
        </DropdownMenu>
    );
};
