import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { LogsheetPreviewCanvas } from "@/modules/logsheets/components/preview-canvas";
import { useTemplate } from "@/modules/templates/api";
import { useState } from "react";
import { useIntl } from "react-intl";

type Props = {
    isOpen: boolean;
    onClose: () => void;
    logsheetId?: string | null;
    templateId: string;
};

export const PreviewModal = ({
    isOpen,
    onClose,
    logsheetId,
    templateId,
}: Props) => {
    const intl = useIntl();
    const [activeSide, setActiveSide] = useState<"front" | "back">("front");
    const templateQuery = useTemplate(templateId);
    const template = templateQuery.data;

    const hasBackside = !!template?.backsideTemplate;

    if (activeSide === "back" && !hasBackside && template) {
        setActiveSide("front");
    }

    const currentTemplateId =
        activeSide === "front"
            ? templateId
            : (template?.backsideTemplate?.id ?? templateId);

    return (
        <Dialog open={isOpen} onOpenChange={onClose}>
            <DialogContent className="max-h-[90vh] min-w-[90vw] flex flex-col">
                <DialogHeader>
                    <div className="flex items-center justify-between pr-8">
                        <DialogTitle>
                            {intl.formatMessage({
                                id: "logsheets.preview.title",
                                defaultMessage: "Logsheet preview",
                            })}
                        </DialogTitle>

                        {hasBackside && (
                            <Tabs
                                value={activeSide}
                                onValueChange={(v) =>
                                    setActiveSide(v as "front" | "back")
                                }
                                className="w-[200px]"
                            >
                                <TabsList className="grid w-full grid-cols-2">
                                    <TabsTrigger value="front">
                                        {intl.formatMessage({
                                            id: "alignment.side.front",
                                            defaultMessage: "Front",
                                        })}
                                    </TabsTrigger>
                                    <TabsTrigger value="back">
                                        {intl.formatMessage({
                                            id: "alignment.side.back",
                                            defaultMessage: "Back",
                                        })}
                                    </TabsTrigger>
                                </TabsList>
                            </Tabs>
                        )}
                    </div>
                </DialogHeader>
                <div className="flex-1 overflow-y-auto min-h-0">
                    <LogsheetPreviewCanvas
                        logsheetId={logsheetId}
                        templateId={currentTemplateId}
                        isBackside={activeSide === "back"}
                    />
                </div>
            </DialogContent>
        </Dialog>
    );
};
