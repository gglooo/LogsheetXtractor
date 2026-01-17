import { Button } from "@/components/ui/button";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { DEFAULT_SCALE, SvgWrapper } from "@/modules/canvas/svg-wrapper";
import { usePdfFileImage } from "@/modules/files/api";
import { AlignmentOverlay } from "@/modules/logsheets/alignment/components/alignment-overlay";
import { AspectRatioIndicator } from "@/modules/logsheets/alignment/components/aspect-ratio-indicator";
import { WarpedTemplateOverlay } from "@/modules/logsheets/alignment/components/warped-template-overlay";
import {
    useAlignLogsheetMutation,
    useAutomaticAlignLogsheetMutation,
} from "@/modules/logsheets/api";
import type { LogsheetType } from "@/modules/logsheets/schema";
import { PdfViewer } from "@/modules/pdf/components/pdf-viewer";
import type { Position } from "@/modules/pdf/hooks/use-draw-rectangle";
import { BotIcon, RotateCcw } from "lucide-react";
import { useState } from "react";
import { useIntl } from "react-intl";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";

type AlignmentEditorProps = {
    logsheet: LogsheetType;
};

const getDefaultCoords = (w: number, h: number): Position[] => [
    { x: w * 0.1 * DEFAULT_SCALE, y: h * 0.1 * DEFAULT_SCALE },
    { x: w * 0.9 * DEFAULT_SCALE, y: h * 0.1 * DEFAULT_SCALE },
    { x: w * 0.9 * DEFAULT_SCALE, y: h * 0.9 * DEFAULT_SCALE },
    { x: w * 0.1 * DEFAULT_SCALE, y: h * 0.9 * DEFAULT_SCALE },
];

export const AlignmentEditor = ({ logsheet }: AlignmentEditorProps) => {
    const intl = useIntl();
    const navigate = useNavigate();
    const [activeSide, setActiveSide] = useState<"front" | "back">("front");

    const [frontCoordinates, setFrontCoordinates] = useState<Position[]>(
        logsheet.alignmentData?.frontside ??
            getDefaultCoords(logsheet.template.width, logsheet.template.height)
    );

    const [backCoordinates, setBackCoordinates] = useState<Position[]>(
        logsheet.backsideTemplate
            ? logsheet.alignmentData?.backside ??
                  getDefaultCoords(
                      logsheet.backsideTemplate.width,
                      logsheet.backsideTemplate.height
                  )
            : []
    );

    const frontFile = usePdfFileImage(logsheet.template.fileId);
    const backFile = usePdfFileImage(logsheet.backsideTemplate?.fileId);

    const templateWidth =
        activeSide === "front"
            ? logsheet.template.width
            : logsheet.backsideTemplate!.width;
    const templateHeight =
        activeSide === "front"
            ? logsheet.template.height
            : logsheet.backsideTemplate!.height;

    const alignMutation = useAlignLogsheetMutation();
    const automaticAlignMutation = useAutomaticAlignLogsheetMutation();

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

    const handleReset = () => {
        if (activeSide === "front") {
            setFrontCoordinates(
                getDefaultCoords(templateWidth, templateHeight)
            );
        } else {
            setBackCoordinates(getDefaultCoords(templateWidth, templateHeight));
        }
    };

    const handleAlignAutomatically = async () => {
        try {
            await automaticAlignMutation.mutateAsync(logsheet.id);

            toast.success(
                intl.formatMessage({
                    id: "logsheets.alignment.automatic.success",
                    defaultMessage: "Automatic alignment successful",
                })
            );
        } catch (error) {
            console.error(error);
            toast.error(
                intl.formatMessage({
                    id: "logsheets.alignment.automatic.error",
                    defaultMessage: "Automatic alignment failed",
                })
            );
        }
    };

    const hasBackside = !!logsheet.backsideTemplate;

    return (
        <div className="flex flex-col h-full">
            <div className="flex items-center justify-between px-6 py-3 bg-white border-b shadow-sm z-10">
                <div className="flex gap-2 items-center justify-center">
                    {hasBackside ? (
                        <Tabs
                            value={activeSide}
                            onValueChange={(value) =>
                                setActiveSide(value as "front" | "back")
                            }
                            className="w-75"
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
                        variant="outline"
                        size="sm"
                        onClick={handleReset}
                        title={intl.formatMessage({
                            id: "alignment.reset",
                            defaultMessage: "Reset",
                        })}
                    >
                        <RotateCcw className="w-4 h-4 mr-2" />
                        {intl.formatMessage({
                            id: "alignment.reset",
                            defaultMessage: "Reset",
                        })}
                    </Button>
                    <Button
                        variant="outline"
                        size="sm"
                        onClick={handleAlignAutomatically}
                        disabled={automaticAlignMutation.isPending}
                    >
                        <BotIcon className="w-4 h-4" />
                        {automaticAlignMutation.isPending
                            ? intl.formatMessage({
                                  id: "alignment.automaticAligning",
                                  defaultMessage: "Aligning...",
                              })
                            : intl.formatMessage({
                                  id: "alignment.automaticAlign",
                                  defaultMessage: "Align automatically",
                              })}
                    </Button>
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
                <SvgWrapper
                    includeHistoryControls={false}
                    includeZoomControls={false}
                >
                    <div className="relative w-full h-full">
                        <PdfViewer
                            fileId={logsheet.file.id}
                            pageNumber={activeSide === "front" ? 1 : 2}
                        />
                        {(activeSide === "front" && frontFile.data) ||
                        (activeSide === "back" && backFile.data) ? (
                            <WarpedTemplateOverlay
                                points={
                                    activeSide === "front"
                                        ? frontCoordinates
                                        : backCoordinates
                                }
                                templateFile={
                                    activeSide === "front"
                                        ? frontFile.data!
                                        : backFile.data!
                                }
                                width={templateWidth}
                                height={templateHeight}
                            />
                        ) : null}
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
                            templateWidth={templateWidth}
                        />
                    </div>
                </SvgWrapper>
            </div>
        </div>
    );
};
