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
import { baseLogsheetsPath } from "@/modules/logsheets/routes";
import { baseTemplateEditorPath } from "@/modules/template-editor/routes";
import { CloneTemplateAction } from "@/modules/templates/actions/clone-template-action";
import {
    useDeleteTemplateMutation,
    useExportConfigMutation,
} from "@/modules/templates/api";
import type { TemplateListItemType } from "@/modules/templates/schema";
import { format } from "date-fns";
import {
    ArrowRightFromLineIcon,
    EditIcon,
    FilesIcon,
    MoreVertical,
    Trash2,
    UploadIcon,
} from "lucide-react";
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
    const exportConfigMutation = useExportConfigMutation();

    const handleDeleteTemplate = async () => {
        try {
            await deleteTemplateMutation.mutateAsync(template.id);
            toast.success(
                intl.formatMessage({
                    id: "templates.actions.delete.success",
                    defaultMessage: "Template deleted successfully!",
                }),
            );
        } catch (error) {
            console.error("Error deleting template:", error);
            toast.error(
                intl.formatMessage({
                    id: "templates.actions.delete.error",
                    defaultMessage: "Failed to delete template",
                }),
            );
        }
    };

    const handleExportConfig = async () => {
        try {
            await exportConfigMutation.mutateAsync({ templateId: template.id });
            toast.success(
                intl.formatMessage({
                    id: "templates.actions.export.success",
                    defaultMessage: "Template config exported.",
                }),
            );
        } catch (error) {
            console.error("Error exporting template config:", error);
            toast.error(
                intl.formatMessage({
                    id: "templates.actions.export.error",
                    defaultMessage: "Failed to export template config",
                }),
            );
        }
    };

    const handleEditTemplate = () => {
        navigate(`${baseTemplateEditorPath}/${template.id}`);
    };

    const handleLogsheetsClick = () => {
        navigate(`/templates/${template.id}/logsheets`);
    };

    const hadleUploadLogsheets = () => {
        navigate(`/templates/${template.id}${baseLogsheetsPath}/upload`);
    };

    return (
        <Card>
            <CardHeader>
                <CardTitle>{template.name}</CardTitle>
                <CardDescription>
                    {format(new Date(template.createdAt), "PPP p")}
                </CardDescription>
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
                            <CloneTemplateAction templateId={template.id} />
                            <DropdownMenuItem onClick={handleExportConfig}>
                                <ArrowRightFromLineIcon className="mr-2 h-4 w-4" />
                                {intl.formatMessage({
                                    id: "templates.actions.export",
                                    defaultMessage: "Export config",
                                })}
                            </DropdownMenuItem>
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
                <div className="flex flex-row lg:flex-row gap-2 w-full">
                    <Button
                        size="sm"
                        className="flex-1 gap-2"
                        onClick={hadleUploadLogsheets}
                    >
                        <UploadIcon />
                        {intl.formatMessage({
                            id: "templates.actions.process",
                            defaultMessage: "Add logsheets",
                        })}
                    </Button>
                </div>
                <div className="flex flex-row lg:flex-row gap-2 w-full mt-2">
                    <Button
                        variant="outline"
                        size="sm"
                        className="flex-1 gap-2"
                        onClick={handleLogsheetsClick}
                    >
                        <FilesIcon />
                        {intl.formatMessage({
                            id: "templates.actions.logsheets",
                            defaultMessage: "Logsheets",
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
                            defaultMessage: "Edit",
                        })}
                    </Button>
                </div>
            </CardContent>
        </Card>
    );
};
