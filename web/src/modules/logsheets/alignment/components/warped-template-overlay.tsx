import { getUrlFromBytes } from "@/lib/utils";
import { useSvgZoom } from "@/modules/canvas/context/svg-zoom-context";
import type { DownloadedFileType } from "@/modules/files/schema";
import { getHomographyMatrix } from "@/modules/logsheets/alignment/utils";
import type { Position } from "@/modules/pdf/hooks/use-draw-rectangle";
import { getScaleFromReferenceScale } from "@/modules/template-editor/utils/coordinates";
import { useMemo } from "react";

type WarpedTemplateOverlayProps = {
    points: Position[];
    templateFile: DownloadedFileType;
    width: number;
    height: number;
    opacity?: number;
};

export const WarpedTemplateOverlay = ({
    points,
    templateFile,
    width: templateWidth,
    height: templateHeight,
    opacity = 0.7,
}: WarpedTemplateOverlayProps) => {
    const { scale, width } = useSvgZoom();

    const transformStyle = useMemo(() => {
        if (!points || points.length < 4) {
            return "none";
        }

        const referenceScale = getScaleFromReferenceScale(
            width,
            scale,
            templateWidth,
        );

        const dstPoints = points.map((p) => ({
            x: p.x * referenceScale,
            y: p.y * referenceScale,
        }));

        const srcPoints = [
            { x: 0, y: 0 },
            { x: templateWidth, y: 0 },
            { x: templateWidth, y: templateHeight },
            { x: 0, y: templateHeight },
        ];

        try {
            return getHomographyMatrix(srcPoints, dstPoints);
        } catch (e) {
            console.error("Failed to calculate homography", e);
            return "none";
        }
    }, [points, width, scale, templateWidth, templateHeight]);

    return (
        <div
            className="absolute top-0 left-0 pointer-events-none origin-top-left will-change-transform"
            style={{
                width: templateWidth,
                height: templateHeight,
                transform: transformStyle,
                transformOrigin: "0 0",
            }}
        >
            <img
                src={getUrlFromBytes(templateFile.bytes)}
                alt=""
                className="w-full h-full object-contain"
                style={{
                    mixBlendMode: "difference",
                    opacity: opacity,
                    filter: "contrast(1.5) grayscale(1)",
                }}
            />
        </div>
    );
};
