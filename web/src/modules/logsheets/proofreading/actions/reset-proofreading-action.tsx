import { Button } from "@/components/ui/button";
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogTrigger,
} from "@/components/ui/dialog";
import { useResetProofreadingMutation } from "@/modules/logsheets/proofreading/api";
import { useState } from "react";
import { useIntl } from "react-intl";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";

export const ResetProofreadingAction = ({
    logsheetId,
}: {
    logsheetId: string;
}) => {
    const intl = useIntl();

    const [isOpen, setIsOpen] = useState(false);

    const resetProofreadingMutation = useResetProofreadingMutation();
    const navigate = useNavigate();

    const handleResetProofreading = async () => {
        try {
            await resetProofreadingMutation.mutateAsync(logsheetId);
            setIsOpen(false);
            toast.success(
                intl.formatMessage({
                    id: "logsheets.proofreading.reset.success",
                    defaultMessage: "Proofreading has been reset.",
                }),
            );
            navigate(-1);
        } catch (error) {
            console.error("Error resetting proofreading:", error);
            toast.error(
                intl.formatMessage({
                    id: "logsheets.proofreading.reset.error",
                    defaultMessage:
                        "An error occurred while resetting proofreading.",
                }),
            );
        }
    };

    return (
        <Dialog open={isOpen} onOpenChange={setIsOpen}>
            <DialogTrigger asChild>
                <Button
                    variant="outline"
                    className="hover:border-destructive hover:bg-destructive/10"
                    onClick={() => setIsOpen(true)}
                >
                    {intl.formatMessage({
                        id: "logsheets.proofreading.reset.action",
                        defaultMessage: "Reset Proofreading",
                    })}
                </Button>
            </DialogTrigger>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>
                        {intl.formatMessage({
                            id: "logsheets.proofreading.reset.title",
                            defaultMessage: "Reset proofreading",
                        })}
                    </DialogTitle>
                </DialogHeader>
                <div className="flex flex-col gap-4">
                    <p>
                        {intl.formatMessage({
                            id: "logsheets.proofreading.reset.confirmation",
                            defaultMessage:
                                "Are you sure you want to reset proofreading? The logsheet will be reset to its original state deleting all extracted values.",
                        })}
                    </p>
                    <div className="flex justify-end gap-2">
                        <Button
                            variant="outline"
                            onClick={() => setIsOpen(false)}
                        >
                            {intl.formatMessage({
                                id: "logsheets.proofreading.reset.cancel",
                                defaultMessage: "Cancel",
                            })}
                        </Button>
                        <Button
                            className="bg-destructive hover:bg-destructive/90"
                            onClick={handleResetProofreading}
                        >
                            {intl.formatMessage({
                                id: "logsheets.proofreading.reset.confirm",
                                defaultMessage: "Reset proofreading",
                            })}
                        </Button>
                    </div>
                </div>
            </DialogContent>
        </Dialog>
    );
};
