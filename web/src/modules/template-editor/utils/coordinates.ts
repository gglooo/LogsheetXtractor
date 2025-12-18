import type { Coordinates } from "@/schema";

export const getScaleFromReferenceScale = (
    width: number,
    scale: number,
    originalWidth: number
): number => {
    return (width * scale) / originalWidth;
};

export const getScaleToReferenceScale = (
    width: number,
    scale: number,
    originalWidth: number
): number => {
    return (1 / scale) * (originalWidth / width);
};

export const scaleCoordinatesToReference = (
    coordinates: Coordinates,
    scale: number
): Coordinates => {
    return {
        x: coordinates.x * scale,
        y: coordinates.y * scale,
        width: coordinates.width * scale,
        height: coordinates.height * scale,
    };
};
