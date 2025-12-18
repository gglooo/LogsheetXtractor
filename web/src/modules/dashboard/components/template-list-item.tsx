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
import { DetectRoisAction } from "@/modules/rois/actions/detect-rois-action";
import { baseTemplateEditorPath } from "@/modules/template-editor/routes";
import { CloneTemplateAction } from "@/modules/templates/actions/clone-template-action";
import { useDeleteTemplateMutation } from "@/modules/templates/api";
import type { TemplateListItemType } from "@/modules/templates/schema";
import { Edit, EditIcon, MoreVertical, Trash2, UploadIcon } from "lucide-react";
import { useIntl } from "react-intl";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";

export const TemplateListItem = ({
    template,
}: {
    template: TemplateListItemType;
}) => {
    const intl = useIntl();
    const navigate = useNavigate();

    const deleteTemplateMutation = useDeleteTemplateMutation();

    const handleDeleteTemplate = async () => {
        try {
            await deleteTemplateMutation.mutateAsync(template.id);
            toast.success(
                intl.formatMessage({
                    id: "templates.actions.delete.success",
                    defaultMessage: "Template deleted successfully!",
                })
            );
        } catch (error) {
            console.error("Error deleting template:", error);
            toast.error(
                intl.formatMessage({
                    id: "templates.actions.delete.error",
                    defaultMessage: "Failed to delete template",
                })
            );
        }
    };

    const handleEditTemplate = () => {
        navigate(`${baseTemplateEditorPath}/${template.id}`);
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
                            <DetectRoisAction
                                inDropdown
                                templateId={template.id}
                            />
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
                        onClick={handleEditTemplate}
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
