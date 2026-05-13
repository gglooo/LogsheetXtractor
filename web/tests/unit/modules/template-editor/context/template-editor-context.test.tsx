import { SelectedRoisProvider } from "@/modules/template-editor/context/selected-rois-context";
import { TemplateEditorProvider } from "@/modules/template-editor/context/template-editor-context";
import { useSelectedRois } from "@/modules/template-editor/hooks/use-selected-rois";
import { useTemplateEditor } from "@/modules/template-editor/hooks/use-template-editor";
import type { TemplateType } from "@/modules/templates/schema";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";

const now = new Date().toISOString();
const templateId = "11111111-1111-4111-8111-111111111111";

const template: TemplateType = {
    id: templateId,
    createdAt: now,
    updatedAt: null,
    deletedAt: null,
    name: "Template",
    parent: null,
    width: 1000,
    height: 1000,
    file: null,
    rois: [],
    residuals: [],
    isEditable: true,
};

const Harness = () => {
    const { rois, addRoi, setRois, removeRoi, undo, redo } =
        useTemplateEditor();
    const { selectedRoiIds, setSelectedRoiIds } = useSelectedRois();

    const selectAll = () => {
        setSelectedRoiIds(rois.map((roi) => roi.id));
    };

    const moveSelected = () => {
        setRois((previous) =>
            previous.map((roi) =>
                selectedRoiIds.includes(roi.id)
                    ? {
                          ...roi,
                          coordinates: {
                              ...roi.coordinates,
                              x: roi.coordinates.x + 20,
                              y: roi.coordinates.y + 10,
                          },
                      }
                    : roi,
            ),
        );
    };

    const resizeSelected = () => {
        setRois((previous) =>
            previous.map((roi) =>
                selectedRoiIds.includes(roi.id)
                    ? {
                          ...roi,
                          coordinates: {
                              ...roi.coordinates,
                              width: roi.coordinates.width + 5,
                              height: roi.coordinates.height + 7,
                          },
                      }
                    : roi,
            ),
        );
    };

    return (
        <div>
            <output aria-label="rois">{JSON.stringify(rois)}</output>
            <output aria-label="selected">{selectedRoiIds.join(",")}</output>
            <button
                onClick={() => {
                    const id = addRoi(
                        { x: 10.4, y: 20.6, width: 30.2, height: 40.8 },
                        "first",
                    );
                    setSelectedRoiIds(id ? [id] : []);
                }}
            >
                add first
            </button>
            <button
                onClick={() => {
                    addRoi(
                        { x: 100, y: 110, width: 120, height: 130 },
                        "second",
                        "Number",
                    );
                }}
            >
                add second
            </button>
            <button onClick={selectAll}>select all</button>
            <button onClick={moveSelected}>move selected</button>
            <button onClick={resizeSelected}>resize selected</button>
            <button
                onClick={() => {
                    selectedRoiIds.forEach(removeRoi);
                    setSelectedRoiIds([]);
                }}
            >
                delete selected
            </button>
            <button onClick={undo}>undo</button>
            <button onClick={redo}>redo</button>
        </div>
    );
};

const renderHarness = () =>
    render(
        <TemplateEditorProvider template={template}>
            <SelectedRoisProvider>
                <Harness />
            </SelectedRoisProvider>
        </TemplateEditorProvider>,
    );

const getRois = () =>
    JSON.parse(screen.getByLabelText("rois").textContent ?? "[]") as Array<{
        id: string;
        variableName: string;
        type: string;
        coordinates: { x: number; y: number; width: number; height: number };
    }>;

describe("TemplateEditorProvider ROI editing workflow", () => {
    it("creates, selects, moves, resizes, deletes, undoes, and redoes ROI edits", async () => {
        const user = userEvent.setup();
        renderHarness();

        await user.click(screen.getByRole("button", { name: "add first" }));

        let rois = getRois();
        expect(rois).toHaveLength(1);
        expect(rois[0].variableName).toMatch(/^first-/);
        expect(rois[0].coordinates).toEqual({
            x: 10,
            y: 21,
            width: 30,
            height: 41,
        });
        expect(screen.getByLabelText("selected").textContent).toBe(rois[0].id);

        await user.click(screen.getByRole("button", { name: "move selected" }));
        expect(getRois()[0].coordinates).toMatchObject({ x: 30, y: 31 });

        await user.click(
            screen.getByRole("button", { name: "resize selected" }),
        );
        expect(getRois()[0].coordinates).toMatchObject({
            width: 35,
            height: 48,
        });

        await user.click(screen.getByRole("button", { name: "delete selected" }));
        expect(getRois()).toHaveLength(0);

        await user.click(screen.getByRole("button", { name: "undo" }));
        expect(getRois()).toHaveLength(1);

        await user.click(screen.getByRole("button", { name: "redo" }));
        expect(getRois()).toHaveLength(0);
    });

    it("tracks multi-select edits and can undo the grouped move", async () => {
        const user = userEvent.setup();
        renderHarness();

        await user.click(screen.getByRole("button", { name: "add first" }));
        await user.click(screen.getByRole("button", { name: "add second" }));
        await user.click(screen.getByRole("button", { name: "select all" }));
        await user.click(screen.getByRole("button", { name: "move selected" }));

        expect(getRois().map((roi) => roi.coordinates.x)).toEqual([30, 120]);

        await user.click(screen.getByRole("button", { name: "undo" }));

        expect(getRois().map((roi) => roi.coordinates.x)).toEqual([10, 100]);
    });
});
