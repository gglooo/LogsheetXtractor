import { Button } from "@/components/ui/button";
import { CheckCircle2 } from "lucide-react";
import { useIntl } from "react-intl";
import { useNavigate } from "react-router-dom";

export const GamifiedComplete = () => {
    const intl = useIntl();
    const navigate = useNavigate();

    return (
        <div className="flex flex-col items-center justify-center h-full gap-6 p-8 text-center">
            <CheckCircle2
                className="h-20 w-20 text-green-500"
                strokeWidth={1.5}
            />
            <div className="flex flex-col gap-2">
                <h2 className="text-2xl font-bold">
                    {intl.formatMessage({
                        id: "gamified.complete.title",
                        defaultMessage: "All done!",
                    })}
                </h2>
                <p className="text-muted-foreground">
                    {intl.formatMessage({
                        id: "gamified.complete.description",
                        defaultMessage:
                            "There are no more extracted values to review.",
                    })}
                </p>
            </div>
            <Button onClick={() => navigate(-1)}>
                {intl.formatMessage({
                    id: "gamified.complete.back",
                    defaultMessage: "Back to logsheets",
                })}
            </Button>
        </div>
    );
};
