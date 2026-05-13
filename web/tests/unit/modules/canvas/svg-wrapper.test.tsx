import {
    DEFAULT_SCALE,
    SvgWrapper,
} from "@/modules/canvas/svg-wrapper";
import { SvgZoomContext } from "@/modules/canvas/context/svg-zoom-context";
import { fireEvent, render, screen } from "@testing-library/react";
import { useContext } from "react";
import { describe, expect, it, vi } from "vitest";

vi.mock("@/modules/template-editor/components/history-controls", () => ({
    HistoryControls: () => <div data-testid="history-controls" />,
}));

const ZoomConsumer = () => {
    const { scale, width } = useContext(SvgZoomContext);
    return (
        <div data-testid="zoom-context">
            {scale}:{width}
        </div>
    );
};

describe("SvgWrapper", () => {
    it("provides zoom context and updates scale from controls", () => {
        render(
            <SvgWrapper>
                <ZoomConsumer />
            </SvgWrapper>,
        );

        expect(screen.getByTestId("history-controls")).toBeInTheDocument();
        expect(screen.getByTestId("zoom-context")).toHaveTextContent(
            `${DEFAULT_SCALE}:0`,
        );

        const [zoomOut, reset, zoomIn] = screen.getAllByRole("button");
        expect(reset).toBeDisabled();

        fireEvent.click(zoomIn);
        expect(screen.getByTestId("zoom-context")).toHaveTextContent("0.8:0");
        expect(reset).not.toBeDisabled();

        fireEvent.click(reset);
        expect(screen.getByTestId("zoom-context")).toHaveTextContent(
            `${DEFAULT_SCALE}:0`,
        );

        fireEvent.click(zoomOut);
        expect(
            Number(screen.getByTestId("zoom-context").textContent?.split(":")[0]),
        ).toBeCloseTo(0.4);
    });

    it("can render without optional controls", () => {
        render(
            <SvgWrapper
                includeHistoryControls={false}
                includeZoomControls={false}
            >
                <ZoomConsumer />
            </SvgWrapper>,
        );

        expect(screen.queryByTestId("history-controls")).not.toBeInTheDocument();
        expect(screen.queryAllByRole("button")).toHaveLength(0);
        expect(screen.getByTestId("zoom-context")).toBeInTheDocument();
    });
});
