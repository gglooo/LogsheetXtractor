import {
    getDuplicates,
    splitRoiHorizontally,
    splitRoiVertically,
} from "@/modules/pdf/utils";
import type { RoiType } from "@/modules/rois/schema";
import { describe, expect, it } from "vitest";

const createRoi = (): RoiType => ({
    id: "roi-1",
    createdAt: new Date().toISOString(),
    updatedAt: null,
    deletedAt: null,
    templateId: "template-1",
    variableName: "field",
    type: "Number",
    coordinates: {
        x: 10,
        y: 20,
        width: 100,
        height: 60,
    },
    validationCondition: null,
});

describe("pdf utils", () => {
    it("splits an ROI vertically at the selected point", () => {
        const [left, right] = splitRoiVertically(createRoi(), { x: 45, y: 35 });

        expect(left).toEqual({ x: 10, y: 20, width: 35, height: 60 });
        expect(right).toEqual({ x: 45, y: 20, width: 65, height: 60 });
    });

    it("splits an ROI horizontally at the selected point", () => {
        const [top, bottom] = splitRoiHorizontally(createRoi(), {
            x: 45,
            y: 50,
        });

        expect(top).toEqual({ x: 10, y: 20, width: 100, height: 30 });
        expect(bottom).toEqual({ x: 10, y: 50, width: 100, height: 30 });
    });

    it("returns unique duplicate values", () => {
        expect([...getDuplicates(["a", "b", "a", "c", "b", "a"])]).toEqual([
            "a",
            "b",
        ]);
    });
});
