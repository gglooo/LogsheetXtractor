import { Dialog, DialogContent } from "@/components/ui/dialog";
import { Skeleton } from "@/components/ui/skeleton";
import { Spinner } from "@/components/ui/spinner";
import { cn, getUrlFromBytes } from "@/lib/utils";
import { useExtractedValueImage } from "@/modules/logsheets/proofreading/api";
import { useState } from "react";

type ExtractedValueImageProps = {
    id: string;
    size?: "sm" | "lg";
    className?: string;
};

export const ExtractedValueImage = ({
    id,
    size = "sm",
    className,
}: ExtractedValueImageProps) => {
    const { data, isLoading, isError } = useExtractedValueImage(id);
    const [open, setOpen] = useState(false);

    if (isLoading) {
        return size === "lg" ? (
            <Skeleton className={cn("w-full h-32 rounded-md", className)} />
        ) : (
            <Spinner />
        );
    }

    if (isError || !data) {
        return (
            <div
                className={cn(
                    "rounded-md bg-muted flex items-center justify-center text-muted-foreground text-xs",
                    size === "lg" ? "w-full h-32" : "w-25 h-25",
                    className,
                )}
            >
                No image
            </div>
        );
    }

    const src = getUrlFromBytes(data.bytes);

    const imgEl = (
        <img
            src={src}
            alt="Extracted value"
            className="max-w-full max-h-full object-contain"
        />
    );

    return (
        <>
            <div
                className={cn(
                    "cursor-pointer bg-muted rounded-md flex items-center justify-center text-muted-foreground text-xs overflow-hidden",
                    size === "lg"
                        ? "w-full max-h-40 cursor-zoom-in"
                        : "w-25 h-25",
                    className,
                )}
                onClick={() => setOpen(true)}
            >
                {imgEl}
            </div>
            <Dialog open={open} onOpenChange={setOpen}>
                <DialogContent
                    className={cn(
                        "flex items-center justify-center",
                        size === "lg" && "max-w-2xl",
                    )}
                >
                    <img
                        src={src}
                        alt="Extracted value"
                        className="max-w-full max-h-[80vh] object-contain"
                    />
                </DialogContent>
            </Dialog>
        </>
    );
};
