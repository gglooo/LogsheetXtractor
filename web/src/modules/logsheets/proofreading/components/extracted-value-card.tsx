import { Card, CardContent } from "@/components/ui/card";
import { Dialog, DialogContent } from "@/components/ui/dialog";
import { Spinner } from "@/components/ui/spinner";
import { cn } from "@/lib/utils";
import { useExtractedValueImage } from "@/modules/logsheets/proofreading/api";
import { ExtractedValueForm } from "@/modules/logsheets/proofreading/components/extracted-value-form";
import type { ExtractedValueType } from "@/modules/logsheets/schema";
import { memo, useState } from "react";

type ExtractedValueCardProps = {
    extractedValue: ExtractedValueType;
    isSelected: boolean;
    onSelect: (roiId: string) => void;
};

const ExtractedValueImage = ({ id }: { id: string }) => {
    const { data, isLoading, isError } = useExtractedValueImage(id);
    const [open, setOpen] = useState(false);

    if (isLoading) {
        return <Spinner />;
    }

    if (isError || !data) {
        return <div>Error loading image</div>;
    }

    const Img = (
        <img
            src={URL.createObjectURL(new Blob([data.bytes]))}
            alt="Extracted Value"
            className="max-w-full max-h-full object-contain"
        />
    );

    return (
        <>
            <div
                className="w-25 h-25 cursor-pointer bg-muted rounded-md flex items-center justify-center text-muted-foreground text-xs"
                onClick={() => setOpen(true)}
            >
                {Img}
            </div>
            <Dialog open={open} onOpenChange={setOpen}>
                <DialogContent className="flex items-center justify-center">
                    {Img}
                </DialogContent>
            </Dialog>
        </>
    );
};

export const ExtractedValueCard = memo(
    ({ extractedValue, isSelected, onSelect }: ExtractedValueCardProps) => {
        return (
            <Card
                onClick={() => onSelect(extractedValue.roiId)}
                className={cn(
                    "cursor-pointer transition-all duration-200 border-2",
                    isSelected
                        ? "border-primary ring-1 ring-primary"
                        : "border-border"
                )}
            >
                <CardContent className="p-4 flex gap-4">
                    <ExtractedValueImage id={extractedValue.id} />
                    <div className="flex-1 gap-4 flex flex-col">
                        <div className="font-medium text-sm text-muted-foreground">
                            {extractedValue.variableName}
                        </div>
                        <ExtractedValueForm extractedValue={extractedValue} />
                    </div>
                    <div className="flex flex-col justify-end"></div>
                </CardContent>
            </Card>
        );
    }
);
