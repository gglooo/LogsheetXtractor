import { SvgZoomContext } from "@/modules/canvas/context/svg-zoom-context";
import { SelectSvgOverlay } from "@/modules/pdf/components/overlay/pdf-select-svg-overlay";
import { DragProvider } from "@/modules/pdf/context/drag-context";
import type { RoiType } from "@/modules/rois/schema";
import { SelectedRoisContext } from "@/modules/template-editor/hooks/use-selected-rois";
import { fireEvent, render, screen } from "@testing-library/react";
import type React from "react";
import { beforeEach, describe, expect, it, vi } from "vitest";

const rois: RoiType[] = [
    {
        id: "roi-1",
        templateId: "template-1",
        variableName: "first",
        type: "Handwritten",
        coordinates: { x: 10, y: 10, width: 20, height: 20 },
        validationCondition: null,
        createdAt: "2026-01-01T00:00:00.000Z",
    },
    {
        id: "roi-2",
        templateId: "template-1",
        variableName: "second",
        type: "Number",
        coordinates: { x: 300, y: 300, width: 20, height: 20 },
        validationCondition: null,
        createdAt: "2026-01-01T00:00:00.000Z",
    },
];

const renderOverlay = ({
    isSelectedRoi = () => false,
    setSelectedRoiIds = vi.fn(),
    dragEnded = vi.fn(),
    resizeEnded = vi.fn(),
    onFinishDrawing = vi.fn(),
}: {
    isSelectedRoi?: (roiId: string) => boolean;
    setSelectedRoiIds?: React.Dispatch<React.SetStateAction<string[]>>;
    dragEnded?: (rois: RoiType[]) => void;
    resizeEnded?: (rois: RoiType[]) => void;
    onFinishDrawing?: (affectedRoiCount: number) => void;
} = {}) => {
    render(
        <SvgZoomContext.Provider value={{ width: 500, scale: 1 }}>
            <SelectedRoisContext.Provider
                value={{
                    selectedRoiIds: rois.filter((roi) => isSelectedRoi(roi.id)).map((roi) => roi.id),
                    isSelectedRoi,
                    setSelectedRoiIds,
                }}
            >
                <DragProvider>
                    <SelectSvgOverlay
                        rois={rois}
                        width={1000}
                        dragEnded={dragEnded}
                        resizeEnded={resizeEnded}
                        onFinishDrawing={onFinishDrawing}
                        render={(
                            roi,
                            onDragStart,
                            onResizeStart,
                            isDragging,
                        ) => (
                            <g key={roi.id}>
                                <rect
                                    data-testid={`roi-${roi.id}`}
                                    data-x={roi.coordinates.x}
                                    data-y={roi.coordinates.y}
                                    data-width={roi.coordinates.width}
                                    data-height={roi.coordinates.height}
                                    data-dragging={isDragging ? "true" : "false"}
                                    onMouseDown={(event) => {
                                        event.stopPropagation();
                                        onDragStart?.(event, roi.id);
                                    }}
                                />
                                <rect
                                    data-testid={`resize-${roi.id}`}
                                    onMouseDown={(event) => {
                                        event.stopPropagation();
                                        onResizeStart?.(event, roi.id);
                                    }}
                                />
                            </g>
                        )}
                    />
                </DragProvider>
            </SelectedRoisContext.Provider>
        </SvgZoomContext.Provider>,
    );

    return { setSelectedRoiIds, dragEnded, resizeEnded, onFinishDrawing };
};

describe("SelectSvgOverlay", () => {
    beforeEach(() => {
        vi.useRealTimers();
    });

    it("selects ROIs overlapped by a drawn selection rectangle", () => {
        const { setSelectedRoiIds, onFinishDrawing } = renderOverlay();
        const svg = document.querySelector("svg")!;

        fireEvent.mouseDown(svg, { clientX: 0, clientY: 0, button: 0 });
        fireEvent.mouseMove(svg, { clientX: 60, clientY: 60 });
        fireEvent.mouseUp(svg);

        expect(setSelectedRoiIds).toHaveBeenCalledWith(["roi-1"]);
        expect(onFinishDrawing).toHaveBeenCalledWith(1);
        expect(svg.querySelector("rect[x='0'][y='0']")).not.toBeInTheDocument();
    });

    it("clears the selection on background click when no gesture is active", () => {
        const { setSelectedRoiIds } = renderOverlay();
        const svg = document.querySelector("svg")!;

        fireEvent.click(svg);

        expect(setSelectedRoiIds).toHaveBeenCalledWith([]);
    });

    it("reports selected ROI coordinates after dragging", () => {
        const { dragEnded } = renderOverlay({
            isSelectedRoi: (roiId) => roiId === "roi-1",
        });
        const svg = document.querySelector("svg")!;

        fireEvent.mouseDown(screen.getByTestId("roi-roi-1"), {
            clientX: 20,
            clientY: 20,
            button: 0,
        });
        fireEvent.mouseMove(svg, { clientX: 30, clientY: 35 });
        fireEvent.mouseMove(svg, { clientX: 40, clientY: 50 });
        fireEvent.mouseUp(svg);

        expect(dragEnded).toHaveBeenCalledWith([
            expect.objectContaining({
                id: "roi-1",
                coordinates: { x: 50, y: 70, width: 20, height: 20 },
            }),
            expect.objectContaining({
                id: "roi-2",
                coordinates: { x: 300, y: 300, width: 20, height: 20 },
            }),
        ]);
    });

    it("normalizes coordinates when resizing past the ROI origin", () => {
        const { resizeEnded } = renderOverlay({
            isSelectedRoi: (roiId) => roiId === "roi-1",
        });
        const svg = document.querySelector("svg")!;

        fireEvent.mouseDown(screen.getByTestId("resize-roi-1"), {
            clientX: 40,
            clientY: 40,
            button: 0,
        });
        fireEvent.mouseMove(svg, { clientX: 30, clientY: 30 });
        fireEvent.mouseMove(svg, { clientX: 20, clientY: 10 });
        fireEvent.mouseUp(svg);

        expect(resizeEnded).toHaveBeenCalledWith([
            expect.objectContaining({
                id: "roi-1",
                coordinates: { x: -10, y: -30, width: 20, height: 40 },
            }),
            expect.objectContaining({
                id: "roi-2",
                coordinates: { x: 300, y: 300, width: 20, height: 20 },
            }),
        ]);
    });
});
