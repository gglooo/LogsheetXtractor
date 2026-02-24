import { Button } from "@/components/ui/button";
import { Dialog, DialogContent } from "@/components/ui/dialog";
import { DropdownMenuItem } from "@/components/ui/dropdown-menu";
import { Spinner } from "@/components/ui/spinner";
import { useDetectRoisMutation } from "@/modules/rois/api";
import type { DetectRoiResponseType } from "@/modules/rois/schema";
import { HatGlasses } from "lucide-react";
import { useIntl } from "react-intl";
import { toast } from "sonner";

export const DetectRoisAction = ({
    onResult,
    templateId,
    inDropdown,
    className,
    disabled,
}: {
    templateId: string;
    inDropdown?: boolean;
    onResult?: (result: DetectRoiResponseType) => void;
    className?: string;
    disabled?: boolean;
}) => {
    const intl = useIntl();

    const detectRoisMutation = useDetectRoisMutation();

    const ButtonContent = (
        <div className="flex items-center gap-2">
            {detectRoisMutation.isPending ? <Spinner /> : <HatGlasses />}
            {intl.formatMessage({
                id: "rois.actions.detectRois",
                defaultMessage: "Detect ROIs",
            })}
        </div>
    );

    const handleDetectRois = async () => {
        try {
            const result = await detectRoisMutation.mutateAsync(templateId);
            onResult?.(result);
            toast.success(
                intl.formatMessage({
                    id: "rois.actions.detectRois.success",
                    defaultMessage: "ROIs detected successfully!",
                }),
            );
        } catch (error) {
            console.error(error);
            toast.error(
                intl.formatMessage({
                    id: "rois.actions.detectRois.error",
                    defaultMessage: "An error occurred while detecting ROIs.",
                }),
            );
        }
    };

    return (
        <>
            {inDropdown ? (
                <DropdownMenuItem
                    onSelect={(e) => {
                        e.preventDefault();
                        handleDetectRois();
                    }}
                    disabled={disabled}
                    className={className}
                >
                    {ButtonContent}
                </DropdownMenuItem>
            ) : (
                <Button
                    variant="outline"
                    onClick={handleDetectRois}
                    className={className}
                    disabled={disabled}
                >
                    {ButtonContent}
                </Button>
            )}
            <Dialog open={detectRoisMutation.isPending}>
                <DialogContent>
                    <div className="p-6">
                        <div className="flex flex-col items-center justify-center gap-4">
                            <Spinner className="h-10 w-10" />
                            <p>
                                {intl.formatMessage({
                                    id: "rois.actions.detectRois.processing",
                                    defaultMessage:
                                        "Detecting ROIs, please wait...",
                                })}
                            </p>
                        </div>
                    </div>
                </DialogContent>
            </Dialog>
        </>
    );
};
