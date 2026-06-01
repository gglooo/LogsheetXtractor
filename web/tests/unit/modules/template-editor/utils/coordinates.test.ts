import { describe, expect, it } from "vitest";
import {
    areCoordinatesOverlapping,
    getCoordinatesFromPositions,
    getScaleFromReferenceScale,
    getScaleToReferenceScale,
    scaleCoordinatesToReference,
} from "@/modules/template-editor/utils/coordinates";

describe("template-editor coordinates utils", () => {
    it("computes scale conversion from reference", () => {
        expect(getScaleFromReferenceScale(1000, 2, 500)).toBe(4);
    });

    it("computes inverse scale conversion to reference", () => {
        expect(getScaleToReferenceScale(1000, 2, 500)).toBe(0.25);
    });

    it("scales coordinate rectangle", () => {
        expect(
            scaleCoordinatesToReference(
                { x: 10, y: 20, width: 30, height: 40 },
                1.5,
            ),
        ).toEqual({ x: 15, y: 30, width: 45, height: 60 });
    });

    it("normalizes rectangle from two points", () => {
        expect(
            getCoordinatesFromPositions(
                { x: 20, y: 40 },
                { x: 5, y: 25 },
            ),
        ).toEqual({ x: 5, y: 25, width: 15, height: 15 });
    });

    it("treats touching edges as overlapping", () => {
        const a = { x: 0, y: 0, width: 10, height: 10 };
        const b = { x: 10, y: 0, width: 5, height: 5 };

        expect(areCoordinatesOverlapping(a, b)).toBe(true);
    });

    it("detects separated rectangles as non-overlapping", () => {
        const a = { x: 0, y: 0, width: 10, height: 10 };
        const b = { x: 11, y: 11, width: 5, height: 5 };

        expect(areCoordinatesOverlapping(a, b)).toBe(false);
    });
});
