import { Button } from "@/components/ui/button";
import {
    Card,
    CardAction,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/components/ui/card";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { CloneTemplateAction } from "@/modules/templates/actions/clone-template-action";
import { useDeleteTemplateMutation } from "@/modules/templates/api";
import type { TemplateListItemType } from "@/modules/templates/schema";
import { Edit, EditIcon, MoreVertical, Trash2, UploadIcon } from "lucide-react";
import { useIntl } from "react-intl";

export const TemplateListItem = ({
    template,
}: {
    template: TemplateListItemType;
}) => {
    const intl = useIntl();

    const deleteTemplateMutation = useDeleteTemplateMutation();

    const handleDeleteTemplate = async () => {
        await deleteTemplateMutation.mutate(template.id);
    };

    return (
        <Card>
            <CardHeader>
                <CardTitle>{template.name}</CardTitle>
                <CardDescription>{template.createdAt}</CardDescription>
                <CardAction>
                    <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                            <Button
                                variant="ghost"
                                size="icon"
                                className="h-8 w-8 -mr-2"
                            >
                                <MoreVertical className="h-4 w-4" />
                            </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end">
                            <DropdownMenuItem onClick={() => {}}>
                                <Edit className="mr-2 h-4 w-4" />
                                {intl.formatMessage({
                                    id: "templates.actions.editRois",
                                    defaultMessage: "Edit ROIs",
                                })}
                            </DropdownMenuItem>
                            <CloneTemplateAction templateId={template.id} />
                            <DropdownMenuItem
                                className="text-destructive"
                                onClick={handleDeleteTemplate}
                            >
                                <Trash2 className="mr-2 h-4 w-4" />
                                {intl.formatMessage({
                                    id: "templates.actions.delete",
                                    defaultMessage: "Delete",
                                })}
                            </DropdownMenuItem>
                        </DropdownMenuContent>
                    </DropdownMenu>
                </CardAction>
            </CardHeader>
            <CardContent>
                <div className="flex flex-row gap-2 w-full">
                    <Button size="sm" className="flex-1 gap-2">
                        <UploadIcon />
                        {intl.formatMessage({
                            id: "templates.actions.processLogsheet",
                            defaultMessage: "Process logsheet",
                        })}
                    </Button>
                    <Button
                        variant="outline"
                        size="sm"
                        className="flex-1 gap-2"
                    >
                        <EditIcon />
                        {intl.formatMessage({
                            id: "templates.actions.edit",
                            defaultMessage: "Edit template",
                        })}
                    </Button>
                </div>
            </CardContent>
        </Card>
    );
};
