import { Spinner } from "@/components/ui/spinner";
import { getUrlFromBytes } from "@/lib/utils";
import { useSvgZoom } from "@/modules/canvas/context/svg-zoom-context";
import { SvgCanvas } from "@/modules/canvas/svg-canvas";
import { SvgWrapper } from "@/modules/canvas/svg-wrapper";
import { useLogsheetImage } from "@/modules/logsheets/api";
import { RoiSvg } from "@/modules/rois/components/roi-svg";
import type { RoiType } from "@/modules/rois/schema";
import { SelectedRoisProvider } from "@/modules/template-editor/context/selected-rois-context";
import { getScaleFromReferenceScale } from "@/modules/template-editor/utils/coordinates";
import { useTemplate } from "@/modules/templates/api";
import { useCallback } from "react";
import { useIntl } from "react-intl";

type RoiSvgCanvasProps = {
    templateId: string;
    shouldDisplay: boolean;
};

const RoiSvgCanvas = ({ templateId, shouldDisplay }: RoiSvgCanvasProps) => {
    const { scale, width } = useSvgZoom();

    const templateQuery = useTemplate(templateId);

    const referenceScale = getScaleFromReferenceScale(
        width,
        scale,
        templateQuery.data?.width ?? 0,
    );

    const renderRoi = useCallback(
        (roi: RoiType) => (
            <RoiSvg
                key={roi.id}
                roi={roi}
                scale={referenceScale}
                isSelected={false}
            />
        ),
        [referenceScale],
    );

    return templateQuery.data && shouldDisplay ? (
        <SvgCanvas
            width={referenceScale}
            render={renderRoi}
            rois={templateQuery.data.rois}
        />
    ) : null;
};

type LogsheetPreviewCanvasProps = {
    logsheetId?: string | null;
    templateId: string;
    isBackside: boolean;
};

export const LogsheetPreviewCanvas = ({
    logsheetId,
    templateId,
    isBackside,
}: LogsheetPreviewCanvasProps) => {
    const intl = useIntl();
    const logsheetImageQuery = useLogsheetImage(logsheetId, isBackside);

    return (
        <SelectedRoisProvider>
            <SvgWrapper includeHistoryControls={false}>
                {logsheetImageQuery.isLoading ? (
                    <div className="flex h-64 w-full items-center justify-center">
                        <Spinner />
                    </div>
                ) : logsheetImageQuery.isError ? (
                    <div className="flex h-64 w-full items-center justify-center p-4 text-center text-muted-foreground">
                        {isBackside
                            ? intl.formatMessage({
                                  id: "logsheets.preview.backsideMissing",
                                  defaultMessage:
                                      "This logsheet does not have a backside page.",
                              })
                            : intl.formatMessage({
                                  id: "logsheets.preview.error",
                                  defaultMessage:
                                      "Failed to load logsheet preview.",
                              })}
                    </div>
                ) : null}
                <div className="w-full relative">
                    {logsheetImageQuery.data ? (
                        <img
                            src={getUrlFromBytes(logsheetImageQuery.data.bytes)}
                            alt="Logsheet"
                        />
                    ) : null}
                    <RoiSvgCanvas
                        templateId={templateId}
                        shouldDisplay={!!logsheetImageQuery.data}
                    />
                </div>
            </SvgWrapper>
        </SelectedRoisProvider>
    );
};
