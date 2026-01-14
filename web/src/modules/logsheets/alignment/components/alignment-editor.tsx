import { Button } from "@/components/ui/button";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { AlignmentOverlay } from "@/modules/logsheets/alignment/components/alignment-overlay";
import { AspectRatioIndicator } from "@/modules/logsheets/alignment/components/aspect-ratio-indicator";
import { useAlignLogsheetMutation } from "@/modules/logsheets/api";
import type { LogsheetType } from "@/modules/logsheets/schema";
import { PdfViewer } from "@/modules/pdf/components/pdf-viewer";
import { PdfWrapper } from "@/modules/pdf/components/pdf-wrapper";
import type { Position } from "@/modules/pdf/hooks/use-draw-rectangle";
import { useState } from "react";
import { useIntl } from "react-intl";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";

type AlignmentEditorProps = {
    logsheet: LogsheetType;
};

export const AlignmentEditor = ({ logsheet }: AlignmentEditorProps) => {
    const intl = useIntl();
    const navigate = useNavigate();

    const [frontCoordinates, setFrontCoordinates] = useState<Position[]>([
        { x: 100, y: 100 },
        { x: 500, y: 100 },
        { x: 500, y: 700 },
        { x: 100, y: 700 },
    ]);

    const [backCoordinates, setBackCoordinates] = useState<Position[]>([
        { x: 100, y: 100 },
        { x: 500, y: 100 },
        { x: 500, y: 700 },
        { x: 100, y: 700 },
    ]);

    const [activeSide, setActiveSide] = useState<"front" | "back">("front");

    const alignMutation = useAlignLogsheetMutation();

    const handleSave = async () => {
        try {
            await alignMutation.mutateAsync({
                logsheetId: logsheet.id,
                frontside: frontCoordinates,
                backside: logsheet.backsideTemplate
                    ? backCoordinates
                    : undefined,
            });
            toast.success(
                intl.formatMessage({
                    id: "logsheets.alignment.success",
                    defaultMessage: "Alignment saved",
                })
            );
            navigate(`/templates/${logsheet.template.id}/logsheets`);
        } catch (error) {
            console.error(error);
            toast.error(
                intl.formatMessage({
                    id: "logsheets.alignment.error",
                    defaultMessage: "Failed to save alignment",
                })
            );
        }
    };

    const hasBackside = !!logsheet.backsideTemplate;

    return (
        <div className="flex flex-col h-full">
            <div className="flex justify-between p-2 bg-muted/20">
                <div className="flex gap-2 items-center justify-center">
                    {hasBackside ? (
                        <Tabs
                            value={activeSide}
                            onValueChange={(value) =>
                                setActiveSide(value as "front" | "back")
                            }
                            className="w-[300px]"
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
                    ) : null}
                </div>
                <div className="flex items-center gap-2">
                    <AspectRatioIndicator
                        coordinates={
                            activeSide === "front"
                                ? frontCoordinates
                                : backCoordinates
                        }
                    />
                    <Button
                        onClick={handleSave}
                        disabled={alignMutation.isPending}
                    >
                        {intl.formatMessage({
                            id: "alignment.save",
                            defaultMessage: "Save alignment",
                        })}
                    </Button>
                </div>
            </div>

            <div className="flex-1 relative overflow-hidden bg-gray-100 flex items-center justify-center mb-6">
                <PdfWrapper
                    includeHistoryControls={false}
                    includeZoomControls={false}
                >
                    <div className="relative w-full h-full">
                        <PdfViewer
                            fileId={logsheet.file.id}
                            pageNumber={activeSide === "front" ? 1 : 2}
                        />
                        <AlignmentOverlay
                            coordinates={
                                activeSide === "front"
                                    ? frontCoordinates
                                    : backCoordinates
                            }
                            onChange={
                                activeSide === "front"
                                    ? setFrontCoordinates
                                    : setBackCoordinates
                            }
                        />
                    </div>
                </PdfWrapper>
            </div>
        </div>
    );
};
