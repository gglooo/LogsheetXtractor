import type { Position } from "@/modules/pdf/hooks/use-draw-rectangle";
import { useIntl } from "react-intl";

const getDistance = (p1: Position, p2: Position) => {
    return Math.sqrt(Math.pow(p2.x - p1.x, 2) + Math.pow(p2.y - p1.y, 2));
};

type AspectRatioIndicatorProps = {
    coordinates: Position[];
};

export const AspectRatioIndicator = ({
    coordinates,
}: AspectRatioIndicatorProps) => {
    const intl = useIntl();
    if (coordinates.length !== 4) {
        return null;
    }

    const topWidth = getDistance(coordinates[0], coordinates[1]);
    const bottomWidth = getDistance(coordinates[3], coordinates[2]);
    const leftHeight = getDistance(coordinates[0], coordinates[3]);
    const rightHeight = getDistance(coordinates[1], coordinates[2]);

    const avgWidth = (topWidth + bottomWidth) / 2;
    const avgHeight = (leftHeight + rightHeight) / 2;

    const ratio = avgWidth / avgHeight;

    const A4_PORTRAIT = 1 / Math.sqrt(2);
    const A4_LANDSCAPE = Math.sqrt(2);

    const diffPortrait = Math.abs(ratio - A4_PORTRAIT);
    const diffLandscape = Math.abs(ratio - A4_LANDSCAPE);

    const isPortrait = diffPortrait < diffLandscape;
    const target = isPortrait ? A4_PORTRAIT : A4_LANDSCAPE;
    const deviation = Math.abs((ratio - target) / target);
    const percent = (deviation * 100).toFixed(1);

    const widthDiff = Math.abs(topWidth - bottomWidth);
    const heightDiff = Math.abs(leftHeight - rightHeight);
    const maxDimension = Math.max(avgWidth, avgHeight);
    const parallelDeviation = (widthDiff + heightDiff) / maxDimension;
    const parallelPercent = (parallelDeviation * 100).toFixed(1);

    let colorClass = "text-green-600 bg-green-100 border-green-200";
    let message = intl.formatMessage({
        id: "alignment.quality.excellent",
        defaultMessage: "Excellent alignment",
    });

    const totalDeviation = deviation + parallelDeviation;

    if (totalDeviation > 0.08) {
        colorClass = "text-red-600 bg-red-100 border-red-200";
        message = intl.formatMessage({
            id: "alignment.quality.poor",
            defaultMessage: "Poor alignment",
        });
    } else if (totalDeviation > 0.04) {
        colorClass = "text-yellow-600 bg-yellow-100 border-yellow-200";
        message = intl.formatMessage({
            id: "alignment.quality.fair",
            defaultMessage: "Fair alignment",
        });
    } else if (totalDeviation > 0.02) {
        colorClass = "text-blue-600 bg-blue-100 border-blue-200";
        message = intl.formatMessage({
            id: "alignment.quality.good",
            defaultMessage: "Good alignment",
        });
    }

    return (
        <div>
            <div
                className={`flex items-center gap-2 px-3 py-1 rounded-md border text-sm font-medium ${colorClass}`}
                title={`Ratio Deviation: ${percent}% | Shape Deviation: ${parallelPercent}%`}
            >
                <span>{message}</span>
                <span className="text-xs opacity-80 gap-1 flex">
                    <span>R: {percent}%</span>
                    <span>S: {parallelPercent}%</span>
                </span>
            </div>
        </div>
    );
};
