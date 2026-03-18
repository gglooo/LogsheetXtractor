import { Badge } from "@/components/ui/badge";
import {
    Tooltip,
    TooltipContent,
    TooltipTrigger,
} from "@/components/ui/tooltip";
import { cn } from "@/lib/utils";
import type { LogsheetStatus } from "@/modules/logsheets/schema";
import { useIntl } from "react-intl";

export const LogsheetStatusBadge = ({
    status,
    errorMessage,
}: {
    status: LogsheetStatus;
    errorMessage?: string | null;
}) => {
    const intl = useIntl();
    const statusStyles: Record<string, { className: string; label: string }> = {
        Pending: {
            className:
                "bg-yellow-100 text-yellow-800 border-yellow-300 dark:bg-yellow-500/15 dark:text-yellow-300 dark:border-yellow-500/20",
            label: intl.formatMessage({
                id: "logsheets.status.pending",
                defaultMessage: "Pending",
            }),
        },
        Completed: {
            className:
                "bg-green-100 text-green-800 border-green-300 dark:bg-green-500/15 dark:text-green-300 dark:border-green-500/20",
            label: intl.formatMessage({
                id: "logsheets.status.completed",
                defaultMessage: "Completed",
            }),
        },
        Failed: {
            className:
                "bg-red-100 text-red-800 border-red-300 dark:bg-red-500/15 dark:text-red-300 dark:border-red-500/20",
            label: intl.formatMessage({
                id: "logsheets.status.failed",
                defaultMessage: "Failed",
            }),
        },
        NeedsReview: {
            className:
                "bg-blue-100 text-blue-800 border-blue-300 dark:bg-blue-500/15 dark:text-blue-300 dark:border-blue-500/20",
            label: intl.formatMessage({
                id: "logsheets.status.needsReview",
                defaultMessage: "Needs review",
            }),
        },
        Processing: {
            className:
                "bg-purple-100 text-purple-800 border-purple-300 dark:bg-purple-500/15 dark:text-purple-300 dark:border-purple-500/20",
            label: intl.formatMessage({
                id: "logsheets.status.processing",
                defaultMessage: "Processing",
            }),
        },
        Aligning: {
            className:
                "bg-orange-100 text-orange-800 border-orange-300 dark:bg-orange-500/15 dark:text-orange-300 dark:border-orange-500/20",
            label: intl.formatMessage({
                id: "logsheets.status.aligning",
                defaultMessage: "Aligning",
            }),
        },
    };

    const config = statusStyles[status] ?? {
        className:
            "bg-gray-100 text-gray-800 border-gray-300 dark:bg-gray-800 dark:text-gray-300 dark:border-gray-700",
        label: intl.formatMessage({
            id: "logsheets.status.unknown",
            defaultMessage: "Unknown",
        }),
    };

    const BadgeComponent = (
        <Badge
            className={cn("whitespace-nowrap font-medium", config.className)}
        >
            {config.label}
        </Badge>
    );

    if (!errorMessage) {
        return BadgeComponent;
    }

    return (
        <Tooltip>
            <TooltipTrigger asChild>{BadgeComponent}</TooltipTrigger>
            <TooltipContent className="max-w-xs">{errorMessage}</TooltipContent>
        </Tooltip>
    );
};
