import {
    Sidebar,
    SidebarContent,
    SidebarFooter,
    SidebarHeader,
} from "@/components/ui/sidebar";
import { ToolsSidebarGroup } from "@/modules/template-editor/sidebar/tools";
import { useIntl } from "react-intl";

interface EditorSidebarProps {
    className?: string;
}

export const EditorSidebar = ({ className }: EditorSidebarProps) => {
    const intl = useIntl();

    return (
        <Sidebar className={className}>
            <SidebarHeader>
                {intl.formatMessage({
                    id: "templateEditor.sidebar.header",
                    defaultMessage: "Template editor",
                })}{" "}
            </SidebarHeader>
            <SidebarContent>
                <ToolsSidebarGroup />
            </SidebarContent>
            <SidebarFooter>Footer</SidebarFooter>
        </Sidebar>
    );
};
