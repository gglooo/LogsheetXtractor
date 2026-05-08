import { Badge } from "@/components/ui/badge";
import {
    ResizableHandle,
    ResizablePanel,
    ResizablePanelGroup,
} from "@/components/ui/resizable";
import { Spinner } from "@/components/ui/spinner";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { SvgWrapper } from "@/modules/canvas/svg-wrapper";
import { VerifyAllExtractedValuesAction } from "@/modules/logsheets/proofreading/actions/verify-all-extracted-values-action";
import {
    ExtractedValuesList,
    type ExtractedValuesListHandle,
} from "@/modules/logsheets/proofreading/components/extracted-values-list";
import { ProofreadingLogsheetViewer } from "@/modules/logsheets/proofreading/components/proofreading-logsheet-viewer";
import { ProofreadingNavbar } from "@/modules/logsheets/proofreading/components/proofreading-navbar";
import { useExtractedValues } from "@/modules/logsheets/proofreading/hooks/use-extracted-values";
import { scrollRoiIntoView } from "@/modules/logsheets/proofreading/utils/scroll";
import { getValidationConditionsByRoiId } from "@/modules/logsheets/proofreading/utils/validation-conditions";
import type { RoiType } from "@/modules/rois/schema";
import type { RoiValidationConditionType } from "@/modules/rois/validation/schema";
import { SelectedRoisProvider } from "@/modules/template-editor/context/selected-rois-context";
import { TemplateEditorProvider } from "@/modules/template-editor/context/template-editor-context";
import { useTemplate } from "@/modules/templates/api";
import { useCallback, useMemo, useRef } from "react";
import { useIntl } from "react-intl";
import { useParams } from "react-router-dom";

export const ProofreadingPage = () => {
    const intl = useIntl();
    const { id, templateId } = useParams<{ id: string; templateId: string }>();

    const {
        logsheet,
        unverifiedExtractedValues,
        verifiedExtractedValues,
        unverifiedExtractedValuesMap,
        isLogsheetLoading,
    } = useExtractedValues(id!);

    const unverifiedListRef = useRef<ExtractedValuesListHandle>(null);

    const { data: template, isLoading: isTemplateLoading } = useTemplate(
        templateId!,
    );

    const { data: backsideTemplate, isLoading: isBackTemplateLoading } =
        useTemplate(template?.backsideTemplate?.id ?? "");

    const handleRoiClick = (roiId: string) => {
        unverifiedListRef.current?.scrollToRoi(roiId);
        scrollRoiIntoView(roiId);
    };

    const handleListRoiClick = (roiId: string) => {
        scrollRoiIntoView(roiId);
    };

    const shouldRenderRoiFn = useCallback(
        (roi: RoiType) => {
            return Boolean(unverifiedExtractedValuesMap[roi.id]);
        },
        [unverifiedExtractedValuesMap],
    );

    const validationConditionsByRoiId = useMemo<
        Record<string, RoiValidationConditionType>
    >(
        () => getValidationConditionsByRoiId(template, backsideTemplate),
        [template, backsideTemplate],
    );

    if (
        isLogsheetLoading ||
        isTemplateLoading ||
        !logsheet ||
        !template ||
        (template.backsideTemplate &&
            isBackTemplateLoading &&
            !backsideTemplate)
    ) {
        return (
            <div className="h-screen w-screen flex items-center justify-center">
                <Spinner />
            </div>
        );
    }

    return (
        <div className="flex flex-col h-screen overflow-hidden bg-background">
            <ProofreadingNavbar
                logsheetId={id!}
                toReviewCount={unverifiedExtractedValues.length}
            />
            <TemplateEditorProvider template={template}>
                <SelectedRoisProvider>
                    <div className="flex-1 overflow-hidden flex flex-row">
                        <ResizablePanelGroup>
                            <ResizablePanel defaultSize={75} minSize={30}>
                                <div className="h-full border-r border-border relative overflow-scroll p-4 bg-muted/30">
                                    <SvgWrapper includeHistoryControls={false}>
                                        <div className="flex flex-col gap-8">
                                            <ProofreadingLogsheetViewer
                                                logsheet={logsheet}
                                                template={template}
                                                rois={template.rois}
                                                onRoiClick={handleRoiClick}
                                                shouldRenderRoiFn={
                                                    shouldRenderRoiFn
                                                }
                                            />
                                            {backsideTemplate ? (
                                                <ProofreadingLogsheetViewer
                                                    logsheet={logsheet}
                                                    template={backsideTemplate}
                                                    rois={backsideTemplate.rois}
                                                    backside={true}
                                                    onRoiClick={handleRoiClick}
                                                    shouldRenderRoiFn={
                                                        shouldRenderRoiFn
                                                    }
                                                />
                                            ) : null}
                                        </div>
                                    </SvgWrapper>
                                </div>
                            </ResizablePanel>
                            <ResizableHandle withHandle />
                            <ResizablePanel defaultSize={50} minSize={350}>
                                <div className="h-full flex flex-col overflow-hidden bg-background">
                                    <Tabs
                                        defaultValue="unverified"
                                        className="flex-1 flex flex-col min-h-0"
                                    >
                                        <div className="p-4 py-2 bg-muted/20 border-b flex items-center justify-between shadow-sm z-10 shrink-0">
                                            <TabsList className="grid w-full grid-cols-2">
                                                <TabsTrigger
                                                    value="unverified"
                                                    className="flex items-center gap-2"
                                                >
                                                    {intl.formatMessage({
                                                        id: "logsheets.proofreading.unverified.tab",
                                                        defaultMessage:
                                                            "Unverified",
                                                    })}
                                                    <Badge
                                                        variant="secondary"
                                                        className="h-5 px-1.5 text-[10px]"
                                                    >
                                                        {
                                                            unverifiedExtractedValues.length
                                                        }
                                                    </Badge>
                                                </TabsTrigger>
                                                <TabsTrigger
                                                    value="verified"
                                                    className="flex items-center gap-2"
                                                >
                                                    {intl.formatMessage({
                                                        id: "logsheets.proofreading.verified.tab",
                                                        defaultMessage:
                                                            "Verified",
                                                    })}
                                                    <Badge
                                                        variant="secondary"
                                                        className="h-5 px-1.5 text-[10px]"
                                                    >
                                                        {
                                                            verifiedExtractedValues.length
                                                        }
                                                    </Badge>
                                                </TabsTrigger>
                                            </TabsList>
                                        </div>
                                        <div className="flex-1 overflow-hidden min-h-0 bg-background relative">
                                            <TabsContent
                                                value="unverified"
                                                className="absolute inset-0 m-0 data-[state=inactive]:hidden flex flex-col"
                                            >
                                                <VerifyAllExtractedValuesAction
                                                    logsheetId={id!}
                                                    unverifiedExtractedValues={
                                                        unverifiedExtractedValues
                                                    }
                                                />
                                                <div className="flex-1 overflow-hidden">
                                                    <ExtractedValuesList
                                                        ref={unverifiedListRef}
                                                        className="h-full"
                                                        extractedValues={
                                                            unverifiedExtractedValues
                                                        }
                                                        validationConditionsByRoiId={
                                                            validationConditionsByRoiId
                                                        }
                                                        onRoiClick={
                                                            handleListRoiClick
                                                        }
                                                    />
                                                </div>
                                            </TabsContent>
                                            <TabsContent
                                                value="verified"
                                                className="absolute inset-0 m-0 data-[state=inactive]:hidden"
                                            >
                                                <ExtractedValuesList
                                                    className="h-full"
                                                    extractedValues={
                                                        verifiedExtractedValues
                                                    }
                                                    validationConditionsByRoiId={
                                                        validationConditionsByRoiId
                                                    }
                                                    onRoiClick={
                                                        handleListRoiClick
                                                    }
                                                />
                                            </TabsContent>
                                        </div>
                                    </Tabs>
                                </div>
                            </ResizablePanel>
                        </ResizablePanelGroup>
                    </div>
                </SelectedRoisProvider>
            </TemplateEditorProvider>
        </div>
    );
};
