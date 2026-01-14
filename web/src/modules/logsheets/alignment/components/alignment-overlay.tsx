import { useEffect, useRef, useState } from "react";

interface AlignmentOverlayProps {
    coordinates: { x: number; y: number }[];
    onChange: (coords: { x: number; y: number }[]) => void;
}

export const AlignmentOverlay = ({
    coordinates,
    onChange,
}: AlignmentOverlayProps) => {
    const [draggingIndex, setDraggingIndex] = useState<number | null>(null);
    const svgRef = useRef<SVGSVGElement>(null);

    const coordinatesRef = useRef(coordinates);
    const draggingIndexRef = useRef(draggingIndex);
    const onChangeRef = useRef(onChange);

    useEffect(() => {
        coordinatesRef.current = coordinates;
    }, [coordinates]);

    useEffect(() => {
        draggingIndexRef.current = draggingIndex;
    }, [draggingIndex]);

    useEffect(() => {
        onChangeRef.current = onChange;
    }, [onChange]);

    const handleMouseDown = (index: number, e: React.MouseEvent) => {
        e.stopPropagation();
        setDraggingIndex(index);
    };

    useEffect(() => {
        const handleMouseMove = (e: MouseEvent) => {
            if (draggingIndexRef.current === null || !svgRef.current) return;

            const CTM = svgRef.current.getScreenCTM();
            if (!CTM) return;

            const x = (e.clientX - CTM.e) / CTM.a;
            const y = (e.clientY - CTM.f) / CTM.d;

            const newCoordinates = [...coordinatesRef.current];
            newCoordinates[draggingIndexRef.current] = { x, y };
            onChangeRef.current(newCoordinates);
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
    }, []);

    return (
        <svg
            ref={svgRef}
            className="absolute inset-0 w-full h-full pointer-events-none"
            style={{ zIndex: 50 }}
        >
            <polygon
                points={coordinates.map((p) => `${p.x},${p.y}`).join(" ")}
                fill="rgba(0, 128, 255, 0.2)"
                stroke="blue"
                strokeWidth="2"
            />
            {coordinates.map((p, index) => (
                <g key={index}>
                    <circle
                        cx={p.x}
                        cy={p.y}
                        r={20}
                        fill="transparent"
                        className="pointer-events-auto cursor-move"
                        onMouseDown={(e) => handleMouseDown(index, e)}
                    />
                    <circle
                        cx={p.x}
                        cy={p.y}
                        r={6}
                        fill="white"
                        stroke="blue"
                        strokeWidth="2"
                        className="pointer-events-none"
                    />
                </g>
            ))}
        </svg>
    );
};
