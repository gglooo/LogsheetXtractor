import { DropdownMenuItem, DropdownMenuSub, DropdownMenuSubContent, DropdownMenuSubTrigger } from "@/components/ui/dropdown-menu";
import { useExportConfigMutation } from "@/modules/templates/api";
import { ArrowRightFromLineIcon } from "lucide-react";
import { useIntl } from "react-intl";
import { toast } from "sonner";

type ExportTarget = {
    id: string;
    label: string;
};

const ExportVariantMenuItems = ({
    templateId,
    onExport,
}: {
    templateId: string;
    onExport: (templateId: string, includeRoiValidations: boolean) => Promise<void>;
}) => {
    const intl = useIntl();

    return (
        <>
            <DropdownMenuItem
                className="max-w-56 whitespace-normal break-words leading-snug"
                onClick={() => onExport(templateId, true)}
            >
                {intl.formatMessage({
                    id: "templates.actions.export.withRoiValidations",
                    defaultMessage: "With ROI validations (incompatible with formHTR)",
                })}
            </DropdownMenuItem>
            <DropdownMenuItem
                className="max-w-56 whitespace-normal break-words leading-snug"
                onClick={() => onExport(templateId, false)}
            >
                {intl.formatMessage({
                    id: "templates.actions.export.withoutRoiValidations",
                    defaultMessage: "Without ROI validations",
                })}
            </DropdownMenuItem>
        </>
    );
};

const ExportTypeSubmenu = ({
    target,
    onExport,
}: {
    target: ExportTarget;
    onExport: (templateId: string, includeRoiValidations: boolean) => Promise<void>;
}) => {
    return (
        <DropdownMenuSub>
            <DropdownMenuSubTrigger className="max-w-56 whitespace-normal break-words leading-snug">
                {target.label}
            </DropdownMenuSubTrigger>
            <DropdownMenuSubContent className="w-56 max-w-[80vw]">
                <ExportVariantMenuItems
                    templateId={target.id}
                    onExport={onExport}
                />
            </DropdownMenuSubContent>
        </DropdownMenuSub>
    );
};

export const ExportTemplateConfigAction = ({
    templateId,
    backsideTemplateId,
}: {
    templateId: string;
    backsideTemplateId: string | null;
}) => {
    const intl = useIntl();
    const exportConfigMutation = useExportConfigMutation();

    const handleExportConfig = async (
        selectedTemplateId: string,
        includeRoiValidations: boolean,
    ) => {
        try {
            await exportConfigMutation.mutateAsync({
                templateId: selectedTemplateId,
                includeRoiValidations,
            });
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

    if (!backsideTemplateId) {
        return (
            <DropdownMenuSub>
                <DropdownMenuSubTrigger className="max-w-56 whitespace-normal break-words leading-snug">
                    <ArrowRightFromLineIcon className="mr-2 h-4 w-4" />
                    {intl.formatMessage({
                        id: "templates.actions.export",
                        defaultMessage: "Export config",
                    })}
                </DropdownMenuSubTrigger>
                <DropdownMenuSubContent className="w-56 max-w-[80vw]">
                    <ExportVariantMenuItems
                        templateId={templateId}
                        onExport={handleExportConfig}
                    />
                </DropdownMenuSubContent>
            </DropdownMenuSub>
        );
    }

    return (
        <DropdownMenuSub>
            <DropdownMenuSubTrigger className="max-w-56 whitespace-normal break-words leading-snug">
                <ArrowRightFromLineIcon className="mr-2 h-4 w-4" />
                {intl.formatMessage({
                    id: "templates.actions.export",
                    defaultMessage: "Export config",
                })}
            </DropdownMenuSubTrigger>
            <DropdownMenuSubContent className="w-56 max-w-[80vw]">
                <ExportTypeSubmenu
                    target={{
                        id: templateId,
                        label: intl.formatMessage({
                            id: "templates.actions.export.frontside",
                            defaultMessage: "Frontside",
                        }),
                    }}
                    onExport={handleExportConfig}
                />
                <ExportTypeSubmenu
                    target={{
                        id: backsideTemplateId,
                        label: intl.formatMessage({
                            id: "templates.actions.export.backside",
                            defaultMessage: "Backside",
                        }),
                    }}
                    onExport={handleExportConfig}
                />
            </DropdownMenuSubContent>
        </DropdownMenuSub>
    );
};
