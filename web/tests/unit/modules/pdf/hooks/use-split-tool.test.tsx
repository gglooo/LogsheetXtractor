import { useSplitTool } from "@/modules/pdf/hooks/use-split-tool";
import type { RoiType } from "@/modules/rois/schema";
import {
    TemplateEditorContext,
    type TemplateEditorContextType,
} from "@/modules/template-editor/hooks/use-template-editor";
import { act, renderHook } from "@testing-library/react";
import type React from "react";
import { describe, expect, it, vi } from "vitest";

const roi: RoiType = {
    id: "roi-1",
    templateId: "template-1",
    variableName: "field",
    type: "Handwritten",
    coordinates: { x: 10, y: 20, width: 100, height: 40 },
    validationCondition: null,
    createdAt: "2026-01-01T00:00:00.000Z",
};

const renderSplitTool = ({
    mode = "split",
    getRelativeCoordinates = vi.fn(() => ({ x: 60, y: 30 })),
    setMode = vi.fn(),
    setRois = vi.fn((updater) => {
        if (typeof updater === "function") {
            updater([roi]);
        }
    }),
    getNewRoi = vi.fn((coordinates) => ({
        ...roi,
        id: `new-${coordinates.x}-${coordinates.y}`,
        coordinates,
    })),
}: {
    mode?: TemplateEditorContextType["mode"];
    getRelativeCoordinates?: (event: React.MouseEvent) => { x: number; y: number } | undefined;
    setMode?: TemplateEditorContextType["setMode"];
    setRois?: TemplateEditorContextType["setRois"];
    getNewRoi?: TemplateEditorContextType["getNewRoi"];
} = {}) => {
    const editorValue = {
        mode,
        setMode,
        rois: [roi],
        setRois,
        residuals: [],
        setResiduals: vi.fn(),
        setRoisAndResiduals: vi.fn(),
        addRoi: vi.fn(),
        addRois: vi.fn(),
        getNewRoi,
        removeRoi: vi.fn(),
        drawRoiType: "Handwritten",
        setDrawRoiType: vi.fn(),
        cycleDrawRoiType: vi.fn(),
        template: undefined,
        undo: vi.fn(),
        redo: vi.fn(),
        canUndo: false,
        canRedo: false,
        roiInputRef: { current: null },
        duplicateRoiNames: new Set<string>(),
        isDirty: false,
        markAsSaved: vi.fn(),
    } satisfies TemplateEditorContextType;

    const wrapper = ({ children }: { children: React.ReactNode }) => (
        <TemplateEditorContext.Provider value={editorValue}>
            {children}
        </TemplateEditorContext.Provider>
    );

    const hook = renderHook(() => useSplitTool(getRelativeCoordinates), {
        wrapper,
    });

    return { ...hook, setMode, setRois, getNewRoi, getRelativeCoordinates };
};

describe("useSplitTool", () => {
    it("splits a wide ROI vertically and returns to select mode", () => {
        const { result, setMode, setRois, getNewRoi } = renderSplitTool();

        act(() => {
            result.current.handleSplit(
                { ctrlKey: false, metaKey: false } as React.MouseEvent,
                "roi-1",
            );
        });

        expect(getNewRoi).toHaveBeenCalledWith({
            x: 10,
            y: 20,
            width: 50,
            height: 40,
        });
        expect(getNewRoi).toHaveBeenCalledWith({
            x: 60,
            y: 20,
            width: 50,
            height: 40,
        });
        expect(setRois).toHaveBeenCalledWith(expect.any(Function));
        expect(setMode).toHaveBeenCalledWith("select");
    });

    it("uses ctrl/meta to invert the split orientation", () => {
        const { result, getNewRoi } = renderSplitTool();

        act(() => {
            result.current.handleSplit(
                { ctrlKey: true, metaKey: false } as React.MouseEvent,
                "roi-1",
            );
        });

        expect(getNewRoi).toHaveBeenCalledWith({
            x: 10,
            y: 20,
            width: 100,
            height: 10,
        });
        expect(getNewRoi).toHaveBeenCalledWith({
            x: 10,
            y: 30,
            width: 100,
            height: 30,
        });
    });

    it("draws split guide lines only for split mode and points inside the ROI", () => {
        const { result, rerender } = renderSplitTool();

        act(() => {
            result.current.setSplitRoiGuideLines(
                { ctrlKey: false, metaKey: false } as React.MouseEvent,
                roi,
            );
        });

        expect(result.current.roiGuideLines).toEqual({
            x: 60,
            y: 20,
            width: 3,
            height: 40,
        });

        const outside = vi.fn(() => ({ x: 500, y: 500 }));
        const next = renderSplitTool({ getRelativeCoordinates: outside });

        act(() => {
            next.result.current.setSplitRoiGuideLines(
                { ctrlKey: false, metaKey: false } as React.MouseEvent,
                roi,
            );
        });

        expect(next.result.current.roiGuideLines).toBeUndefined();

        rerender();
    });

    it("does nothing when the ROI or mouse coordinates are unavailable", () => {
        const { result, setRois, getRelativeCoordinates } = renderSplitTool({
            getRelativeCoordinates: vi.fn(() => undefined),
        });

        act(() => {
            result.current.handleSplit(
                { ctrlKey: false, metaKey: false } as React.MouseEvent,
                "missing-roi",
            );
            result.current.handleSplit(
                { ctrlKey: false, metaKey: false } as React.MouseEvent,
                "roi-1",
            );
        });

        expect(getRelativeCoordinates).toHaveBeenCalledTimes(1);
        expect(setRois).not.toHaveBeenCalled();
    });
});
