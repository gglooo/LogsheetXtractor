import type { Coordinates } from "@/schema";

export const sortRoisByPosition = <T extends { coordinates: Coordinates }>(
    rois: T[]
): T[] => {
    return rois.slice().sort((a, b) => {
        if (a.coordinates.y === b.coordinates.y) {
            return a.coordinates.x - b.coordinates.x;
        }
        return a.coordinates.y - b.coordinates.y;
    });
};
