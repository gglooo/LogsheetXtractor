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
    DropdownMenuSub,
    DropdownMenuSubContent,
    DropdownMenuSubTrigger,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { TemplateEditButton } from "@/modules/dashboard/components/template-edit-button";
import { baseLogsheetsPath } from "@/modules/logsheets/routes";
import { CloneTemplateAction } from "@/modules/templates/actions/clone-template-action";
import {
    useDeleteTemplateMutation,
    useExportConfigMutation,
} from "@/modules/templates/api";
import type { TemplateListItemType } from "@/modules/templates/schema";
import { format } from "date-fns";
import {
    ArrowRightFromLineIcon,
    FilesIcon,
    MoreVertical,
    Trash2,
    UploadIcon,
} from "lucide-react";
import { FormattedPlural, useIntl } from "react-intl";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";

import { Badge } from "@/components/ui/badge";
import { useDateFnsLocale } from "@/lib/hooks/useDateFnsLocale";
import { TemplatePreviewImage } from "@/modules/dashboard/components/template-preview-image";
import { baseTemplateEditorPath } from "@/modules/template-editor/routes";

export const TemplateListItem = ({
    template,
}: {
    template: TemplateListItemType;
}) => {
    const intl = useIntl();
    const locale = useDateFnsLocale();
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

    const handleExportConfig = async (id: string) => {
        try {
            await exportConfigMutation.mutateAsync({ templateId: id });
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

    const handleLogsheetsClick = () => {
        navigate(`/templates/${template.id}/logsheets`);
    };

    const hadleUploadLogsheets = () => {
        navigate(`/templates/${template.id}${baseLogsheetsPath}/upload`);
    };

    const handleCardClick = (e: React.MouseEvent) => {
        const target = e.target as HTMLElement;
        const isInteractive = target.closest(
            'button, a, [role="menuitem"], [role="menu"]',
        );

        if (isInteractive) {
            return;
        }

        navigate(`${baseTemplateEditorPath}/${template.id}`);
    };

    return (
        <Card
            className="flex flex-col overflow-hidden hover:bg-card/20 transition-colors duration-200 cursor-pointer"
            onClick={handleCardClick}
        >
            <TemplatePreviewImage
                templateId={template.id}
                templateName={template.name}
                fileId={template.fileId}
            />
            <CardHeader className="flex-none">
                <CardTitle className="hyphens-auto text-lg font-bold">
                    {template.name}
                </CardTitle>
                <CardDescription className="mt-1 gap-2 flex flex-col">
                    <div className="flex gap-2">
                        <Badge>
                            {template.roiCount}{" "}
                            <FormattedPlural
                                value={template.roiCount}
                                one={intl.formatMessage({
                                    id: "templates.roi",
                                    defaultMessage: "ROI",
                                })}
                                other={intl.formatMessage({
                                    id: "templates.rois",
                                    defaultMessage: "ROIs",
                                })}
                            />
                        </Badge>
                        {template.backsideTemplateId ? (
                            <Badge>
                                {intl.formatMessage({
                                    id: "templates.with-backside",
                                    defaultMessage: "With backside",
                                })}
                            </Badge>
                        ) : null}
                    </div>
                    {format(new Date(template.createdAt), "PPP p", {
                        locale,
                    })}
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
                            {template.backsideTemplateId ? (
                                <DropdownMenuSub>
                                    <DropdownMenuSubTrigger>
                                        <ArrowRightFromLineIcon className="mr-2 h-4 w-4" />
                                        {intl.formatMessage({
                                            id: "templates.actions.export",
                                            defaultMessage: "Export config",
                                        })}
                                    </DropdownMenuSubTrigger>
                                    <DropdownMenuSubContent>
                                        <DropdownMenuItem
                                            onClick={() =>
                                                handleExportConfig(template.id)
                                            }
                                        >
                                            {intl.formatMessage({
                                                id: "templates.actions.export.frontside",
                                                defaultMessage: "Frontside",
                                            })}
                                        </DropdownMenuItem>
                                        <DropdownMenuItem
                                            onClick={() =>
                                                handleExportConfig(
                                                    template.backsideTemplateId!,
                                                )
                                            }
                                        >
                                            {intl.formatMessage({
                                                id: "templates.actions.export.backside",
                                                defaultMessage: "Backside",
                                            })}
                                        </DropdownMenuItem>
                                    </DropdownMenuSubContent>
                                </DropdownMenuSub>
                            ) : (
                                <DropdownMenuItem
                                    onClick={() =>
                                        handleExportConfig(template.id)
                                    }
                                >
                                    <ArrowRightFromLineIcon className="mr-2 h-4 w-4" />
                                    {intl.formatMessage({
                                        id: "templates.actions.export",
                                        defaultMessage: "Export config",
                                    })}
                                </DropdownMenuItem>
                            )}
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
            <CardContent className="flex flex-col justify-end flex-1">
                <div className="flex flex-row lg:flex-row gap-2 w-full">
                    <Button
                        variant="outline"
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
                        })}{" "}
                        ({template.logsheetCount})
                    </Button>
                    <TemplateEditButton template={template} />
                </div>
            </CardContent>
        </Card>
    );
};
