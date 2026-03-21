import { useSvgZoom } from "@/modules/canvas/context/svg-zoom-context";
import type { Position } from "@/modules/pdf/hooks/use-draw-rectangle";
import { getScaleFromReferenceScale } from "@/modules/template-editor/utils/coordinates";
import { useCallback, useEffect, useRef, useState } from "react";

type AlignmentOverlayProps = {
    coordinates: Position[];
    onChange: (coords: Position[]) => void;
    templateWidth: number;
};

const POINT_LABELS = ["TL", "TR", "BR", "BL"];

const EDGE_INDEX_START = 4;
const ALL_POINTS_INDEX = -1;

export const AlignmentOverlay = ({
    coordinates,
    onChange,
    templateWidth,
}: AlignmentOverlayProps) => {
    const [draggingIndex, setDraggingIndex] = useState<number | null>(null);
    const svgRef = useRef<SVGSVGElement>(null);
    const { scale, width } = useSvgZoom();

    const referenceScale = getScaleFromReferenceScale(
        width,
        scale,
        templateWidth,
    );

    const lastMousePositionRef = useRef<Position | null>(null);
    const coordinatesRef = useRef(coordinates);
    const draggingIndexRef = useRef(draggingIndex);
    const onChangeRef = useRef(onChange);
    const scaleRef = useRef(referenceScale);

    useEffect(() => {
        coordinatesRef.current = coordinates;
    }, [coordinates]);
    useEffect(() => {
        draggingIndexRef.current = draggingIndex;
    }, [draggingIndex]);
    useEffect(() => {
        onChangeRef.current = onChange;
    }, [onChange]);
    useEffect(() => {
        scaleRef.current = scale;
    }, [scale]);

    const getCoordinates = useCallback(
        (clientX: number, clientY: number) => {
            if (!svgRef.current) {
                return null;
            }

            const rect = svgRef.current.getBoundingClientRect();
            const currentScale = rect.width / templateWidth;

            return {
                x: (clientX - rect.left) / currentScale,
                y: (clientY - rect.top) / currentScale,
            };
        },
        [svgRef, templateWidth],
    );

    const handleMouseDown = (index: number, e: React.MouseEvent) => {
        e.stopPropagation();

        const startPos = getCoordinates(e.clientX, e.clientY);

        if (startPos) {
            lastMousePositionRef.current = startPos;
        }

        setDraggingIndex(index);
    };

    useEffect(() => {
        const handleMouseMove = (e: MouseEvent) => {
            if (draggingIndexRef.current === null || !svgRef.current) return;

            const currentPos = getCoordinates(e.clientX, e.clientY);
            if (!currentPos || !lastMousePositionRef.current) {
                return;
            }

            const dx = currentPos.x - lastMousePositionRef.current.x;
            const dy = currentPos.y - lastMousePositionRef.current.y;
            const newCoords = [...coordinatesRef.current];

            const id = draggingIndexRef.current;

            if (id === ALL_POINTS_INDEX) {
                // Move All
                for (let i = 0; i < coordinates.length; i++) {
                    newCoords[i] = {
                        x: newCoords[i].x + dx,
                        y: newCoords[i].y + dy,
                    };
                }
            } else if (id >= EDGE_INDEX_START) {
                // Move edge
                const edgeIndex = id - EDGE_INDEX_START;
                const p1 = edgeIndex;
                const p2 = (edgeIndex + 1) % EDGE_INDEX_START;
                newCoords[p1] = {
                    x: newCoords[p1].x + dx,
                    y: newCoords[p1].y + dy,
                };
                newCoords[p2] = {
                    x: newCoords[p2].x + dx,
                    y: newCoords[p2].y + dy,
                };
            } else {
                // Move point
                newCoords[id] = {
                    x: newCoords[id].x + dx,
                    y: newCoords[id].y + dy,
                };
            }

            onChange(newCoords);
            lastMousePositionRef.current = currentPos;
        };

        const handleMouseUp = () => {
            if (draggingIndexRef.current !== null) {
                setDraggingIndex(null);
            }
        };

        window.addEventListener("mousemove", handleMouseMove);
        window.addEventListener("mouseup", handleMouseUp);

        return () => {
            window.removeEventListener("mousemove", handleMouseMove);
            window.removeEventListener("mouseup", handleMouseUp);
        };
    }, [coordinates.length, getCoordinates, onChange]);

    const renderEdges = () => {
        const edges = [];
        for (let i = 0; i < coordinates.length; i++) {
            const p1 = coordinates[i];
            const p2 = coordinates[(i + 1) % coordinates.length];
            edges.push(
                <line
                    key={`edge-${i}`}
                    x1={p1.x * referenceScale}
                    y1={p1.y * referenceScale}
                    x2={p2.x * referenceScale}
                    y2={p2.y * referenceScale}
                    stroke="#3b82f6"
                    strokeWidth="3"
                    className="cursor-grab hover:stroke-blue-400 transition-colors"
                    onMouseDown={(e) =>
                        handleMouseDown(i + EDGE_INDEX_START, e)
                    }
                />,
            );
        }
        return edges;
    };

    const activePoint =
        draggingIndex !== null &&
        draggingIndex >= 0 &&
        draggingIndex < EDGE_INDEX_START
            ? coordinates[draggingIndex]
            : null;

    return (
        <svg
            ref={svgRef}
            className="absolute inset-0 w-full h-full"
            style={{ zIndex: 50 }}
        >
            {activePoint ? (
                <g className="opacity-85 pointer-events-none">
                    <line
                        x1={-10000}
                        y1={activePoint.y * referenceScale}
                        x2={10000}
                        y2={activePoint.y * referenceScale}
                        stroke="#ef4444"
                        strokeWidth="1"
                        strokeDasharray="5,5"
                    />
                    <line
                        x1={activePoint.x * referenceScale}
                        y1={-10000}
                        x2={activePoint.x * referenceScale}
                        y2={10000}
                        stroke="#ef4444"
                        strokeWidth="1"
                        strokeDasharray="5,5"
                    />
                </g>
            ) : null}

            <polygon
                points={coordinates
                    .map(
                        (p) =>
                            `${p.x * referenceScale},${p.y * referenceScale}`,
                    )
                    .join(" ")}
                fill="rgba(0, 128, 255, 0.2)"
                strokeWidth="2"
                className="pointer-events-auto cursor-move z-20"
                onMouseDown={(e) => handleMouseDown(-1, e)}
            />
            <g className="pointer-events-auto">{renderEdges()}</g>
            {coordinates.map((p, index) => (
                <g key={index}>
                    <circle
                        cx={p.x * referenceScale}
                        cy={p.y * referenceScale}
                        r={100}
                        fill="transparent"
                        className="pointer-events-auto cursor-move z-30"
                        onMouseDown={(e) => handleMouseDown(index, e)}
                    />
                    <circle
                        cx={p.x * referenceScale}
                        cy={p.y * referenceScale}
                        r={6}
                        fill="white"
                        stroke="blue"
                        strokeWidth="2"
                        className="pointer-events-none"
                    />
                    <text
                        x={p.x * referenceScale + 12}
                        y={p.y * referenceScale - 12}
                        fill="#1e293b"
                        fontSize="12"
                        fontWeight="bold"
                        className="pointer-events-none select-none bg-white"
                        style={{ textShadow: "0px 0px 4px white" }}
                    >
                        {POINT_LABELS[index]}
                    </text>
                </g>
            ))}
        </svg>
    );
};
