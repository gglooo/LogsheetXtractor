import { SvgZoomContext } from "@/modules/canvas/context/svg-zoom-context";
import { DrawablePdfViewer } from "@/modules/pdf/components/drawable-pdf-viewer";
import { ReadonlyRoiPdfViewer } from "@/modules/pdf/components/readonly-roi-pdf-viewer";
import type { RoiType } from "@/modules/rois/schema";
import { SelectedRoisContext } from "@/modules/template-editor/hooks/use-selected-rois";
import {
    TemplateEditorContext,
    type TemplateEditorContextType,
} from "@/modules/template-editor/hooks/use-template-editor";
import type { TemplateType } from "@/modules/templates/schema";
import { fireEvent, render, screen } from "@testing-library/react";
import type React from "react";
import { describe, expect, it, vi } from "vitest";

vi.mock("@/modules/pdf/components/pdf-viewer", () => ({
    PdfViewer: ({ fileId }: { fileId: string }) => (
        <div data-testid="pdf-viewer" data-file-id={fileId} />
    ),
}));

vi.mock("@/modules/canvas/svg-canvas", () => ({
    SvgCanvas: ({
        rois,
        render,
        dragEnded,
        resizeEnded,
        onFinishDrawing,
    }: {
        rois: RoiType[];
        render: (roi: RoiType) => React.ReactNode;
        dragEnded?: (rois: RoiType[]) => void;
        resizeEnded?: (rois: RoiType[]) => void;
        onFinishDrawing?: (affectedRoiCount: number) => void;
    }) => (
        <div data-testid="svg-canvas">
            {rois.map((roi) => render(roi))}
            <button onClick={() => dragEnded?.(rois)}>finish drag</button>
            <button onClick={() => resizeEnded?.(rois)}>finish resize</button>
            <button onClick={() => onFinishDrawing?.(1)}>finish drawing</button>
        </div>
    ),
}));

vi.mock("@/modules/pdf/components/overlay/pdf-drawing-svg-overlay", () => ({
    PdfDrawingSvgOverlay: ({
        rois,
        render,
    }: {
        rois: RoiType[];
        render: (roi: RoiType) => React.ReactNode;
    }) => (
        <div data-testid="drawing-overlay">{rois.map((roi) => render(roi))}</div>
    ),
}));

vi.mock("@/modules/rois/components/roi-svg", () => ({
    RoiSvg: ({
        roi,
        onRoiClick,
        onRoiDrag,
        onRoiResizeStart,
        isSelected,
        isResizeable,
        guideLineCoordinates,
    }: {
        roi: RoiType;
        onRoiClick?: (event: React.MouseEvent, roiId: string) => void;
        onRoiDrag?: (event: React.MouseEvent, roiId: string) => void;
        onRoiResizeStart?: (event: React.MouseEvent, roiId: string) => void;
        isSelected?: boolean;
        isResizeable?: boolean;
        guideLineCoordinates?: unknown;
    }) => (
        <button
            data-testid={`roi-${roi.id}`}
            data-selected={isSelected ? "true" : "false"}
            data-resizeable={isResizeable ? "true" : "false"}
            data-guide={guideLineCoordinates ? "true" : "false"}
            onClick={(event) => onRoiClick?.(event, roi.id)}
            onMouseDown={(event) => onRoiDrag?.(event, roi.id)}
            onDoubleClick={(event) => onRoiResizeStart?.(event, roi.id)}
        >
            {roi.variableName}
        </button>
    ),
}));

vi.mock(
    "@/modules/template-editor/hooks/use-select-tool-empty-selection-help",
    () => ({
        useSelectToolEmptySelectionHelp: () => ({
            trackSelectionResult: vi.fn(),
        }),
    }),
);

vi.mock(
    "@/modules/template-editor/sidebar/roi-validation/hooks/use-roi-validation-preset-context-menu",
    () => ({
        useRoiValidationPresetContextMenu: () => ({
            handleOpenRoiContextMenu: vi.fn(),
            menuProps: {},
        }),
    }),
);

vi.mock(
    "@/modules/template-editor/sidebar/roi-validation/components/roi-validation-preset-context-menu",
    () => ({
        RoiValidationPresetContextMenu: () => <div data-testid="roi-menu" />,
    }),
);

const roi: RoiType = {
    id: "roi-1",
    templateId: "template-1",
    variableName: "first",
    type: "Handwritten",
    coordinates: { x: 10, y: 20, width: 30, height: 40 },
    validationCondition: null,
    createdAt: "2026-01-01T00:00:00.000Z",
};

const template: TemplateType = {
    id: "template-1",
    createdAt: "2026-01-01T00:00:00.000Z",
    updatedAt: null,
    deletedAt: null,
    name: "Template",
    parent: null,
    width: 1000,
    height: 1400,
    file: null,
    rois: [roi],
    residuals: [],
    isEditable: true,
};

const renderWithEditor = (
    ui: React.ReactNode,
    {
        mode = "select",
        setRois = vi.fn(),
        selectedRoiIds = [],
        setSelectedRoiIds = vi.fn(),
    }: {
        mode?: TemplateEditorContextType["mode"];
        setRois?: TemplateEditorContextType["setRois"];
        selectedRoiIds?: string[];
        setSelectedRoiIds?: (ids: string[]) => void;
    } = {},
) => {
    const editorValue = {
        mode,
        setMode: vi.fn(),
        rois: [roi],
        setRois,
        residuals: [],
        setResiduals: vi.fn(),
        setRoisAndResiduals: vi.fn(),
        addRoi: vi.fn(),
        addRois: vi.fn(),
        getNewRoi: vi.fn(),
        removeRoi: vi.fn(),
        drawRoiType: "Handwritten",
        setDrawRoiType: vi.fn(),
        cycleDrawRoiType: vi.fn(),
        template,
        undo: vi.fn(),
        redo: vi.fn(),
        canUndo: false,
        canRedo: false,
        roiInputRef: { current: null },
        duplicateRoiNames: new Set<string>(),
        isDirty: false,
        markAsSaved: vi.fn(),
    } satisfies TemplateEditorContextType;

    return render(
        <SvgZoomContext.Provider value={{ width: 500, scale: 1 }}>
            <TemplateEditorContext.Provider value={editorValue}>
                <SelectedRoisContext.Provider
                    value={{
                        selectedRoiIds,
                        isSelectedRoi: (roiId) => selectedRoiIds.includes(roiId),
                        setSelectedRoiIds,
                    }}
                >
                    {ui}
                </SelectedRoisContext.Provider>
            </TemplateEditorContext.Provider>
        </SvgZoomContext.Provider>,
    );
};

describe("PDF viewer wrappers", () => {
    it("renders drawable mode with the drawing overlay", () => {
        renderWithEditor(<DrawablePdfViewer fileId="file-1" template={template} />, {
            mode: "draw",
        });

        expect(screen.getByTestId("pdf-viewer")).toHaveAttribute(
            "data-file-id",
            "file-1",
        );
        expect(screen.getByTestId("drawing-overlay")).toBeInTheDocument();
        expect(screen.queryByTestId("svg-canvas")).not.toBeInTheDocument();
    });

    it("renders selectable ROIs and commits canvas drag and resize updates", () => {
        const setRois = vi.fn();

        renderWithEditor(<DrawablePdfViewer fileId="file-1" template={template} />, {
            mode: "select",
            selectedRoiIds: ["roi-1"],
            setRois,
        });

        expect(screen.getByTestId("svg-canvas")).toBeInTheDocument();
        expect(screen.getByTestId("roi-roi-1")).toHaveAttribute(
            "data-resizeable",
            "true",
        );

        fireEvent.click(screen.getByText("finish drag"));
        fireEvent.click(screen.getByText("finish resize"));

        expect(setRois).toHaveBeenCalledTimes(2);
        expect(setRois).toHaveBeenCalledWith([roi]);
    });

    it("selects and filters ROIs in readonly mode while keeping the selected ROI visible", () => {
        const onRoiClick = vi.fn();
        const setSelectedRoiIds = vi.fn();

        renderWithEditor(
            <ReadonlyRoiPdfViewer
                fileId="file-1"
                template={template}
                onRoiClick={onRoiClick}
                shouldRenderRoiFn={() => false}
            />,
            {
                selectedRoiIds: ["roi-1"],
                setSelectedRoiIds,
            },
        );

        fireEvent.click(screen.getByTestId("roi-roi-1"));

        expect(setSelectedRoiIds).toHaveBeenCalledWith(["roi-1"]);
        expect(onRoiClick).toHaveBeenCalledWith("roi-1");
    });
});
