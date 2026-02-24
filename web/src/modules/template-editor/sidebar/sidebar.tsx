import { Separator } from "@/components/ui/separator";
import {
    Sidebar,
    SidebarContent,
    SidebarHeader,
} from "@/components/ui/sidebar";
import { SelectedRoiSidebarGroup } from "@/modules/template-editor/sidebar/selected-roi";
import { ToolsSidebarGroup } from "@/modules/template-editor/sidebar/tools";
import { useIntl } from "react-intl";

interface EditorSidebarProps {
    className?: string;
}

export const EditorSidebar = ({ className }: EditorSidebarProps) => {
    const intl = useIntl();

    return (
        <Sidebar className={className}>
            <SidebarHeader className="text-lg font-medium mt-2">
                {intl.formatMessage({
                    id: "templateEditor.sidebar.header",
                    defaultMessage: "Editor tools",
                })}
            </SidebarHeader>
            <SidebarContent className="px-2 flex flex-col gap-2">
                <ToolsSidebarGroup />
                <Separator />
                <SelectedRoiSidebarGroup />
            </SidebarContent>
        </Sidebar>
    );
};
