import { Button } from "@/components/ui/button";
import { useIntl } from "react-intl";

export const ProofreadingNavbar = () => {
    const intl = useIntl();

    return (
        <div className="border-b h-14 flex items-center justify-between px-4 bg-background shrink-0">
            <h1 className="font-semibold text-lg">
                {intl.formatMessage({
                    id: "proofreading.title",
                    defaultMessage: "Proofreading",
                })}
            </h1>
            <div className="flex items-center gap-2">
                <Button>
                    {intl.formatMessage({
                        id: "proofreading.save",
                        defaultMessage: "Save & Verify",
                    })}
                </Button>
            </div>
        </div>
    );
};
