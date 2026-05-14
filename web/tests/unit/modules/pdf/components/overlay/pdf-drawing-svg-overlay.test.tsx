import { SvgZoomContext } from "@/modules/canvas/context/svg-zoom-context";
import { PdfDrawingSvgOverlay } from "@/modules/pdf/components/overlay/pdf-drawing-svg-overlay";
import type { RoiType } from "@/modules/rois/schema";
import { SelectedRoisContext } from "@/modules/template-editor/hooks/use-selected-rois";
import {
    TemplateEditorContext,
    type TemplateEditorContextType,
} from "@/modules/template-editor/hooks/use-template-editor";
import type { TemplateType } from "@/modules/templates/schema";
import { fireEvent, render } from "@testing-library/react";
import type React from "react";
import { describe, expect, it, vi } from "vitest";

const template: TemplateType = {
    id: "11111111-1111-4111-8111-111111111111",
    createdAt: "2026-01-01T00:00:00.000Z",
    updatedAt: null,
    deletedAt: null,
    name: "Template",
    parent: null,
    width: 1000,
    height: 1400,
    file: null,
    rois: [],
    residuals: [],
    isEditable: true,
};

const renderOverlay = ({
    mode = "draw",
    addRoi = vi.fn(() => "new-roi"),
    setSelectedRoiIds = vi.fn(),
}: {
    mode?: TemplateEditorContextType["mode"];
    addRoi?: TemplateEditorContextType["addRoi"];
    setSelectedRoiIds?: React.Dispatch<React.SetStateAction<string[]>>;
} = {}) => {
    const editorValue = {
        mode,
        setMode: vi.fn(),
        rois: [],
        setRois: vi.fn(),
        residuals: [],
        setResiduals: vi.fn(),
        setRoisAndResiduals: vi.fn(),
        addRoi,
        addRois: vi.fn(),
        getNewRoi: vi.fn(),
        removeRoi: vi.fn(),
        drawRoiType: "Barcode",
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

    render(
        <SvgZoomContext.Provider value={{ width: 500, scale: 1 }}>
            <TemplateEditorContext.Provider value={editorValue}>
                <SelectedRoisContext.Provider
                    value={{
                        selectedRoiIds: [],
                        isSelectedRoi: () => false,
                        setSelectedRoiIds,
                    }}
                >
                    <PdfDrawingSvgOverlay
                        rois={[]}
                        render={(roi: RoiType) => (
                            <g key={roi.id} data-testid={`roi-${roi.id}`} />
                        )}
                    />
                </SelectedRoisContext.Provider>
            </TemplateEditorContext.Provider>
        </SvgZoomContext.Provider>,
    );

    return { addRoi, setSelectedRoiIds };
};

describe("PdfDrawingSvgOverlay", () => {
    it("creates a scaled ROI and selects it after a meaningful draw gesture", () => {
        const { addRoi, setSelectedRoiIds } = renderOverlay();
        const svg = document.querySelector("svg")!;

        fireEvent.mouseDown(svg, { clientX: 10, clientY: 20, button: 0 });
        fireEvent.mouseMove(svg, { clientX: 60, clientY: 80 });

        const preview = svg.querySelector("rect");
        expect(preview).toHaveAttribute("x", "10");
        expect(preview).toHaveAttribute("y", "20");
        expect(preview).toHaveAttribute("width", "50");
        expect(preview).toHaveAttribute("height", "60");

        fireEvent.mouseUp(svg);

        expect(addRoi).toHaveBeenCalledWith(
            { x: 20, y: 40, width: 100, height: 120 },
            undefined,
            "Barcode",
        );
        expect(setSelectedRoiIds).toHaveBeenCalledWith(["new-roi"]);
    });

    it("does not draw or create ROIs when the editor is not in draw mode", () => {
        const { addRoi } = renderOverlay({ mode: "select" });
        const svg = document.querySelector("svg")!;

        fireEvent.mouseDown(svg, { clientX: 10, clientY: 20, button: 0 });
        fireEvent.mouseMove(svg, { clientX: 60, clientY: 80 });
        fireEvent.mouseUp(svg);

        expect(addRoi).not.toHaveBeenCalled();
        expect(svg.querySelector("rect")).not.toBeInTheDocument();
    });

    it("ignores tiny draw gestures", () => {
        const { addRoi } = renderOverlay();
        const svg = document.querySelector("svg")!;

        fireEvent.mouseDown(svg, { clientX: 10, clientY: 20, button: 0 });
        fireEvent.mouseMove(svg, { clientX: 13, clientY: 24 });
        fireEvent.mouseUp(svg);

        expect(addRoi).not.toHaveBeenCalled();
    });
});
