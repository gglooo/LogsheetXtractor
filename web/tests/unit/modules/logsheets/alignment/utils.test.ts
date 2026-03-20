import { describe, expect, it } from "vitest";
import { getHomographyMatrix } from "@/modules/logsheets/alignment/utils";

const parseMatrix3d = (matrix: string) => {
    const numbers = matrix
        .replace("matrix3d(", "")
        .replace(")", "")
        .split(",")
        .map((part) => Number.parseFloat(part.trim()));

    return numbers;
};

describe("alignment utils", () => {
    it("returns a matrix3d string", () => {
        const src = [
            { x: 0, y: 0 },
            { x: 100, y: 0 },
            { x: 100, y: 100 },
            { x: 0, y: 100 },
        ];
        const dst = [...src];

        const matrix = getHomographyMatrix(src, dst);

        expect(matrix.startsWith("matrix3d(")).toBe(true);
        expect(matrix.endsWith(")")).toBe(true);
    });

    it("produces identity transform when source equals destination", () => {
        const src = [
            { x: 0, y: 0 },
            { x: 100, y: 0 },
            { x: 100, y: 100 },
            { x: 0, y: 100 },
        ];

        const matrix = parseMatrix3d(getHomographyMatrix(src, src));

        expect(matrix).toHaveLength(16);

        const expected = [
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1,
        ];

        matrix.forEach((value, index) => {
            expect(value).toBeCloseTo(expected[index], 6);
        });
    });
});
