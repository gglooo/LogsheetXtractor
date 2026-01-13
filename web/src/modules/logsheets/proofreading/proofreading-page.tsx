import {
    ResizableHandle,
    ResizablePanel,
    ResizablePanelGroup,
} from "@/components/ui/resizable";
import { Spinner } from "@/components/ui/spinner";
import { useLogsheet } from "@/modules/logsheets/api";
import { EXTRACTED_VALUE_CARD_ID_PREFIX } from "@/modules/logsheets/proofreading/components/extracted-value-card";
import { ExtractedValuesList } from "@/modules/logsheets/proofreading/components/extracted-values-list";
import { ProofreadingNavbar } from "@/modules/logsheets/proofreading/components/proofreading-navbar";
import type { ExtractedValueType } from "@/modules/logsheets/schema";
import { PdfWrapper } from "@/modules/pdf/components/pdf-wrapper";
import { ReadonlyRoiPdfViewer } from "@/modules/pdf/components/readonly-roi-pdf-viewer";
import type { RoiType } from "@/modules/rois/schema";
import { SelectedRoisProvider } from "@/modules/template-editor/context/selected-rois-context";
import { TemplateEditorProvider } from "@/modules/template-editor/context/template-editor-context";
import { useTemplate } from "@/modules/templates/api";
import { useCallback, useMemo } from "react";
import { useParams } from "react-router-dom";

export const ProofreadingPage = () => {
    const { id, templateId } = useParams<{ id: string; templateId: string }>();

    const { data: logsheet, isLoading: isLogsheetLoading } = useLogsheet(id!);

    const { data: template, isLoading: isTemplateLoading } = useTemplate(
        templateId!
    );

    const unverifiedExtractedValues = useMemo(
        () =>
            logsheet?.extractedValues.reduce((acc, ev) => {
                if (ev.status === "Unverified") {
                    acc[ev.roiId] = ev;
                }

                return acc;
            }, {} as Record<string, ExtractedValueType>) ?? {},
        [logsheet]
    );

    const handleRoiClick = (roiId: string) => {
        const element = document.getElementById(
            `${EXTRACTED_VALUE_CARD_ID_PREFIX}${roiId}`
        );
        if (!element) {
            return;
        }

        element.scrollIntoView({ behavior: "instant", block: "center" });
    };

    const shouldRenderRoiFn = useCallback(
        (roi: RoiType) => {
            return Boolean(unverifiedExtractedValues[roi.id]);
        },
        [unverifiedExtractedValues]
    );

    if (isLogsheetLoading || isTemplateLoading || !logsheet || !template) {
        return (
            <div className="h-screen w-screen flex items-center justify-center">
                <Spinner />
            </div>
        );
    }

    return (
        <div className="flex flex-col h-screen overflow-hidden bg-background">
            <ProofreadingNavbar />
            <TemplateEditorProvider template={template}>
                <SelectedRoisProvider>
                    <div className="flex-1 overflow-hidden flex flex-row">
                        <ResizablePanelGroup>
                            <ResizablePanel defaultSize={75} minSize={30}>
                                <div className="h-full border-r border-border relative overflow-scroll p-4 bg-muted/30">
                                    <PdfWrapper includeHistoryControls={false}>
                                        <ReadonlyRoiPdfViewer
                                            fileId={logsheet.file.id}
                                            template={template}
                                            onRoiClick={handleRoiClick}
                                            shouldRenderRoiFn={
                                                shouldRenderRoiFn
                                            }
                                        />
                                    </PdfWrapper>
                                </div>
                            </ResizablePanel>
                            <ResizableHandle withHandle />
                            <ResizablePanel defaultSize={50} minSize={350}>
                                <div className="h-full overflow-hidden bg-background">
                                    <ExtractedValuesList
                                        extractedValues={Object.values(
                                            unverifiedExtractedValues
                                        )}
                                    />
                                </div>
                            </ResizablePanel>
                        </ResizablePanelGroup>
                    </div>
                </SelectedRoisProvider>
            </TemplateEditorProvider>
        </div>
    );
};
