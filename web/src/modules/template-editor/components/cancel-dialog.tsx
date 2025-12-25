import { Button } from "@/components/ui/button";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";
import { useIntl } from "react-intl";

type CancelDialogProps = {
    open: boolean;
    onCancel: () => void;
    onClose: () => void;
};

export function CancelDialog({ open, onCancel, onClose }: CancelDialogProps) {
    const intl = useIntl();

    return (
        <Dialog open={open} onOpenChange={onClose}>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>
                        {intl.formatMessage({
                            id: "cancelDialog.title",
                            defaultMessage: "Cancel changes?",
                        })}
                    </DialogTitle>
                    <DialogDescription>
                        {intl.formatMessage({
                            id: "cancelDialog.description",
                            defaultMessage:
                                "Are you sure you want to cancel? Unsaved changes will be lost.",
                        })}
                    </DialogDescription>
                </DialogHeader>
                <DialogFooter>
                    <Button variant="outline" onClick={onClose}>
                        {intl.formatMessage({
                            id: "cancelDialog.stay",
                            defaultMessage: "Stay",
                        })}
                    </Button>
                    <Button variant="destructive" onClick={onCancel}>
                        {intl.formatMessage({
                            id: "cancelDialog.cancel",
                            defaultMessage: "Cancel",
                        })}
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}
