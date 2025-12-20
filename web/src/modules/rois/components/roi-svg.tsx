import type { DetectedRoiType } from "@/modules/rois/schema";
import { useState } from "react";

type Props = {
    roi: DetectedRoiType;
    scale: number;
    isSelected?: boolean;
    onDelete: (id: string) => void;
    onRoiClick?: (e: React.MouseEvent, id: string) => void;
};

export const RoiSvg = ({
    roi,
    scale,
    isSelected,
    onDelete,
    onRoiClick,
}: Props) => {
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
            onClick={(e: React.MouseEvent) => roi.id && onRoiClick?.(e, roi.id)}
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
                stroke={isSelected ? "rgb(255, 0, 0)" : "rgb(59, 130, 246)"}
                strokeWidth="2"
                className="transition-colors pointer-events-auto"
            />

            <text
                x={scaledX}
                y={scaledY - 5}
                className="text-[16px] font-bold fill-red-600 select-none pointer-events-none"
            ></text>

            <svg width="100%" height="100%">
                <defs>
                    <filter x="0" y="0" width="1" height="1" id="solid">
                        <feFlood floodColor="yellow" />
                        <feComposite in="SourceGraphic" operator="xor" />
                    </filter>
                </defs>
                <text
                    filter="url(#solid)"
                    x={scaledX}
                    y={scaledY - 5}
                    className="text-[16px] font-bold select-none pointer-events-none"
                >
                    {" "}
                    {roi.variableName}
                </text>
                <text
                    x={scaledX}
                    y={scaledY - 5}
                    className="text-[16px] font-bold select-none pointer-events-none"
                >
                    {roi.variableName}
                </text>
            </svg>

            {isHovered && (
                <g
                    className="cursor-pointer"
                    onClick={(e) => {
                        e.stopPropagation();
                        onDelete(roi.id ?? "unnamed_roi");
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
