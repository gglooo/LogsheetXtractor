import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";
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

type Props = {
    isOpen: boolean;
    onClose: () => void;
    logsheetId: string;
    templateId: string;
};

const RoiSvgCanvas = ({
    templateId,
    shouldDisplay,
}: {
    templateId: string;
    shouldDisplay: boolean;
}) => {
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

export const PreviewModal = ({
    isOpen,
    onClose,
    logsheetId,
    templateId,
}: Props) => {
    const intl = useIntl();

    const logsheetImageQuery = useLogsheetImage(logsheetId);

    return (
        <Dialog open={isOpen} onOpenChange={onClose}>
            <DialogContent className="max-h-[90vh] min-w-[90vw] flex flex-col">
                <DialogHeader>
                    <DialogTitle>
                        {intl.formatMessage({
                            id: "logsheets.preview.title",
                            defaultMessage: "Logsheet preview",
                        })}
                    </DialogTitle>
                </DialogHeader>
                <div className="flex-1 overflow-y-auto min-h-0">
                    <SelectedRoisProvider>
                        <SvgWrapper includeHistoryControls={false}>
                            {logsheetImageQuery.isLoading ? (
                                <div className="flex h-64 w-full items-center justify-center">
                                    <Spinner />
                                </div>
                            ) : logsheetImageQuery.isError ? (
                                <div className="p-4">
                                    {intl.formatMessage({
                                        id: "logsheets.preview.error",
                                        defaultMessage:
                                            "Failed to load logsheet preview.",
                                    })}
                                </div>
                            ) : null}
                            <div className="w-full relative">
                                {logsheetImageQuery.data ? (
                                    <img
                                        src={getUrlFromBytes(
                                            logsheetImageQuery.data!.bytes,
                                        )}
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
                </div>
            </DialogContent>
        </Dialog>
    );
};
