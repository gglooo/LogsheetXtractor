import { SvgCanvas } from "@/modules/canvas/svg-canvas";
import type { RoiType } from "@/modules/rois/schema";
import { render, screen } from "@testing-library/react";
import type React from "react";
import { describe, expect, it, vi } from "vitest";

vi.mock("@/modules/pdf/context/drag-context", () => ({
    DragProvider: ({ children }: { children: React.ReactNode }) => (
        <div data-testid="drag-provider">{children}</div>
    ),
}));

vi.mock("@/modules/pdf/components/overlay/pdf-select-svg-overlay", () => ({
    SelectSvgOverlay: ({
        rois,
        render,
        width,
        dragEnded,
        resizeEnded,
        onFinishDrawing,
    }: {
        rois: RoiType[];
        render: (roi: RoiType) => React.ReactNode;
        width: number;
        dragEnded?: (rois: RoiType[]) => void;
        resizeEnded?: (rois: RoiType[]) => void;
        onFinishDrawing?: (affectedRoiCount: number) => void;
    }) => (
        <div data-testid="select-overlay" data-width={width}>
            {rois.map((roi) => render(roi))}
            <button onClick={() => dragEnded?.(rois)}>drag ended</button>
            <button onClick={() => resizeEnded?.(rois)}>resize ended</button>
            <button onClick={() => onFinishDrawing?.(rois.length)}>
                finish drawing
            </button>
        </div>
    ),
}));

const roi: RoiType = {
    id: "roi-1",
    templateId: "template-1",
    variableName: "field",
    type: "Handwritten",
    coordinates: { x: 10, y: 20, width: 30, height: 40 },
    validationCondition: null,
    createdAt: "2026-01-01T00:00:00.000Z",
};

describe("SvgCanvas", () => {
    it("wraps the select overlay with drag context and forwards overlay callbacks", () => {
        const dragEnded = vi.fn();
        const resizeEnded = vi.fn();
        const onFinishDrawing = vi.fn();

        render(
            <SvgCanvas
                rois={[roi]}
                width={640}
                dragEnded={dragEnded}
                resizeEnded={resizeEnded}
                onFinishDrawing={onFinishDrawing}
                render={(item) => (
                    <span data-testid={`roi-${item.id}`}>
                        {item.variableName}
                    </span>
                )}
            />,
        );

        expect(screen.getByTestId("drag-provider")).toBeInTheDocument();
        expect(screen.getByTestId("select-overlay")).toHaveAttribute(
            "data-width",
            "640",
        );
        expect(screen.getByTestId("roi-roi-1")).toHaveTextContent("field");

        screen.getByText("drag ended").click();
        screen.getByText("resize ended").click();
        screen.getByText("finish drawing").click();

        expect(dragEnded).toHaveBeenCalledWith([roi]);
        expect(resizeEnded).toHaveBeenCalledWith([roi]);
        expect(onFinishDrawing).toHaveBeenCalledWith(1);
    });
});
