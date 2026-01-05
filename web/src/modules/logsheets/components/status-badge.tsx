import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";
import type { LogsheetStatus } from "@/modules/logsheets/schema";

export const LogsheetStatusBadge = ({ status }: { status: LogsheetStatus }) => {
    const statusStyles: Record<string, { className: string; label: string }> = {
        Pending: {
            className:
                "bg-yellow-100 text-yellow-800 border-yellow-300 dark:bg-yellow-500/15 dark:text-yellow-300 dark:border-yellow-500/20",
            label: "Pending",
        },
        Completed: {
            className:
                "bg-green-100 text-green-800 border-green-300 dark:bg-green-500/15 dark:text-green-300 dark:border-green-500/20",
            label: "Completed",
        },
        Failed: {
            className:
                "bg-red-100 text-red-800 border-red-300 dark:bg-red-500/15 dark:text-red-300 dark:border-red-500/20",
            label: "Failed",
        },
        NeedsReview: {
            className:
                "bg-blue-100 text-blue-800 border-blue-300 dark:bg-blue-500/15 dark:text-blue-300 dark:border-blue-500/20",
            label: "Needs review",
        },
    };

    const config = statusStyles[status] ?? {
        className:
            "bg-gray-100 text-gray-800 border-gray-300 dark:bg-gray-800 dark:text-gray-300 dark:border-gray-700",
        label: "Unknown",
    };

    return (
        <Badge
            variant="outline"
            className={cn("whitespace-nowrap font-medium", config.className)}
        >
            {config.label}
        </Badge>
    );
};
