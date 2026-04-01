import { Badge } from "@/components/ui/badge";
import { getLocalDate } from "@/lib/utils";
import { formatDate } from "date-fns";
import { useIntl } from "react-intl";

export const ProcessedBadge = ({
    processedAt,
}: {
    processedAt: Date | string | null;
}) => {
    const intl = useIntl();

    if (processedAt) {
        return (
            <Badge className="bg-green-100 text-green-800 border-green-300 dark:bg-green-500/15 dark:text-green-300 dark:border-green-500/20">
                {intl.formatMessage(
                    {
                        id: "logsheets.processedBadge.processed",
                        defaultMessage: "{processedAt}",
                    },
                    {
                        processedAt: formatDate(
                            getLocalDate(processedAt),
                            "d. M. yyyy, HH:mm",
                        ),
                    },
                )}
            </Badge>
        );
    }

    return (
        <Badge className="bg-yellow-100 text-yellow-800 border-yellow-300 dark:bg-yellow-500/15 dark:text-yellow-300 dark:border-yellow-500/20">
            {intl.formatMessage({
                id: "logsheets.processedBadge.not-processed",
                defaultMessage: "Not processed",
            })}
        </Badge>
    );
};
