import type { DetectedRoiType } from "@/modules/rois/schema";
import { useState } from "react";

type Props = {
    roi: DetectedRoiType;
    scale: number;
    onDelete: (id: string) => void;
};

export const RoiSvg = ({ roi, scale, onDelete }: Props) => {
    const [isHovered, setIsHovered] = useState(false);
    const { x, y, width, height } = roi.coordinates;

    const scaledX = x * scale;
    const scaledY = y * scale;
    const scaledWidth = width * scale;
    const scaledHeight = height * scale;

    return (
        <g
            onMouseEnter={() => setIsHovered(true)}
            onMouseLeave={() => setIsHovered(false)}
            className="cursor-pointer"
        >
            <rect
                x={scaledX}
                y={scaledY}
                width={scaledWidth}
                height={scaledHeight}
                fill={
                    isHovered
                        ? "rgba(59, 130, 246, 0.2)"
                        : "rgba(59, 130, 246, 0.1)"
                }
                stroke="rgb(59, 130, 246)"
                strokeWidth="2"
                className="transition-colors pointer-events-auto"
            />

            <text
                x={scaledX}
                y={scaledY - 5}
                className="text-[12px] font-bold fill-blue-600 select-none pointer-events-none"
            >
                {roi.variableName}
            </text>

            {isHovered && (
                <g
                    className="cursor-pointer"
                    onClick={(e) => {
                        e.stopPropagation();
                        onDelete(roi.variableName);
                    }}
                >
                    <circle
                        cx={scaledX + scaledWidth}
                        cy={scaledY}
                        r="10"
                        className="fill-red-500 hover:fill-red-600 shadow"
                    />
                    <text
                        x={scaledX + scaledWidth}
                        y={scaledY + 4}
                        textAnchor="middle"
                        className="fill-white text-[12px] font-bold pointer-events-none"
                    >
                        ×
                    </text>
                </g>
            )}
        </g>
    );
};
