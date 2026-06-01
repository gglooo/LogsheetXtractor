import type { Position } from "@/modules/pdf/hooks/use-draw-rectangle";
import type { Coordinates } from "@/schema";

export const getScaleFromReferenceScale = (
    width: number,
    scale: number,
    originalWidth: number,
): number => {
    return (width * scale) / originalWidth;
};

export const getScaleToReferenceScale = (
    width: number,
    scale: number,
    originalWidth: number,
): number => {
    return (1 / scale) * (originalWidth / width);
};

export const scaleCoordinatesToReference = (
    coordinates: Coordinates,
    scale: number,
): Coordinates => {
    return {
        x: coordinates.x * scale,
        y: coordinates.y * scale,
        width: coordinates.width * scale,
        height: coordinates.height * scale,
    };
};

export const getCoordinatesFromPositions = (
    startPos: Position,
    currentPos: Position,
): Coordinates => {
    const x = Math.min(startPos.x, currentPos.x);
    const y = Math.min(startPos.y, currentPos.y);
    const width = Math.abs(startPos.x - currentPos.x);
    const height = Math.abs(startPos.y - currentPos.y);

    return { x, y, width, height };
};

export const areCoordinatesOverlapping = (
    coord1: Coordinates,
    coord2: Coordinates,
): boolean => {
    return !(
        coord1.x + coord1.width < coord2.x ||
        coord2.x + coord2.width < coord1.x ||
        coord1.y + coord1.height < coord2.y ||
        coord2.y + coord2.height < coord1.y
    );
};
