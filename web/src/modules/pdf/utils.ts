import type { RoiType } from "@/modules/rois/schema";
import type { Coordinates } from "@/schema";

export const getDuplicates = <T>(arr: T[]) => {
    const seen = new Set<T>();
    const duplicates = new Set<T>();

    for (const item of arr) {
        if (seen.has(item)) {
            duplicates.add(item);
        } else {
            seen.add(item);
        }
    }

    return duplicates;
};

export type Point = {
    x: number;
    y: number;
};

export const splitRoiVertically = (
    roi: RoiType,
    splitPoint: Point
): [Coordinates, Coordinates] => {
    const { x, y, width, height } = roi.coordinates;
    const xDistance = Math.abs(splitPoint.x - x);

    const leftCoords: Coordinates = {
        x,
        y,
        width: xDistance,
        height,
    };

    const rightCoords: Coordinates = {
        x: splitPoint.x,
        y,
        width: width - xDistance,
        height,
    };

    return [leftCoords, rightCoords];
};

export const splitRoiHorizontally = (
    roi: RoiType,
    splitPoint: Point
): [Coordinates, Coordinates] => {
    const { x, y, width, height } = roi.coordinates;
    const yDistance = Math.abs(splitPoint.y - y);

    const topCoords: Coordinates = {
        x,
        y,
        width,
        height: yDistance,
    };

    const bottomCoords: Coordinates = {
        x,
        y: splitPoint.y,
        width,
        height: height - yDistance,
    };

    return [topCoords, bottomCoords];
};
