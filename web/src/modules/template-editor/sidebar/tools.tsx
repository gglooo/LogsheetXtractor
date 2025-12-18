import { Button } from "@/components/ui/button";
import { SidebarGroup } from "@/components/ui/sidebar";
import { DetectRoisAction } from "@/modules/rois/actions/detect-rois-action";
import type { DetectRoiResponseType } from "@/modules/rois/schema";
import { useTemplateEditor } from "@/modules/template-editor/hooks/use-template-editor";
import { MousePointer, Square } from "lucide-react";
import { useIntl } from "react-intl";
import { useParams } from "react-router-dom";

export const ToolsSidebarGroup = () => {
    const intl = useIntl();

    const { id } = useParams<{ id: string }>();

    const { setResiduals, setRois, setMode, mode } = useTemplateEditor();

    const handleSetDetectedData = (detectedData: DetectRoiResponseType) => {
        setRois(detectedData.rois);
        setResiduals(detectedData.residuals);
    };

    return (
        <SidebarGroup
            title={intl.formatMessage({
                id: "templateEditor.sidebar.tools",
                defaultMessage: "Tools",
            })}
        >
            <div className="flex flex-col gap-2 p-2">
                <div className="flex flex-row gap-2">
                    <Button
                        className="w-1/2"
                        variant={mode === "select" ? "default" : "outline"}
                        onClick={() => setMode("select")}
                    >
                        <MousePointer className="mr-2" />
                        {intl.formatMessage({
                            id: "templateEditor.sidebar.selectTool",
                            defaultMessage: "Select",
                        })}
                    </Button>
                    <Button
                        className="w-1/2"
                        variant={mode === "draw" ? "default" : "outline"}
                        onClick={() => setMode("draw")}
                    >
                        <Square className="mr-2" />
                        {intl.formatMessage({
                            id: "templateEditor.sidebar.drawTool",
                            defaultMessage: "Draw",
                        })}
                    </Button>
                </div>
                <DetectRoisAction
                    templateId={id!}
                    onResult={handleSetDetectedData}
                />
            </div>
        </SidebarGroup>
    );
};
