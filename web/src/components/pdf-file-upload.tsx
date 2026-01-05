import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { cn, formatFileSize } from "@/lib/utils";
import { FileIcon, Upload, X } from "lucide-react";
import { useRef, useState } from "react";
import { useIntl } from "react-intl";

export const PdfFileUpload = ({
    file,
    onFileChange,
    className,
    multiple = false,
}: {
    file: File | File[] | null;
    onFileChange: (file: File | File[] | null) => void;
    className?: string;
    multiple?: boolean;
}) => {
    const intl = useIntl();
    const [isDragging, setIsDragging] = useState(false);
    const inputRef = useRef<HTMLInputElement>(null);

    const handleDrag = (e: React.DragEvent) => {
        e.preventDefault();
        e.stopPropagation();
    };

    const handleDragIn = (e: React.DragEvent) => {
        e.preventDefault();
        e.stopPropagation();
        if (e.dataTransfer.items && e.dataTransfer.items.length > 0) {
            setIsDragging(true);
        }
    };

    const handleDragOut = (e: React.DragEvent) => {
        e.preventDefault();
        e.stopPropagation();

        if (e.currentTarget.contains(e.relatedTarget as Node)) {
            return;
        }
        setIsDragging(false);
    };

    const handleDrop = (e: React.DragEvent) => {
        handleDrag(e);
        setIsDragging(false);

        const droppedFiles = e.dataTransfer.files;
        if (!droppedFiles || droppedFiles.length <= 0) {
            return;
        }

        const pdfFiles = Array.from(droppedFiles).filter(
            (file) => file.type === "application/pdf"
        );

        if (multiple) {
            onFileChange(pdfFiles);
        } else if (pdfFiles.length > 0) {
            onFileChange(pdfFiles[0]);
        }
    };

    const handleRemove = (e: React.MouseEvent, index?: number) => {
        e.preventDefault();
        e.stopPropagation();

        if (multiple && Array.isArray(file) && typeof index === "number") {
            const newFiles = file.filter((_, i) => i !== index);
            onFileChange(newFiles.length > 0 ? newFiles : null);
        } else {
            onFileChange(null);
        }
    };

    const handleClick = () => {
        inputRef.current?.click();
    };

    const displayFile = () => {
        if (!file) {
            return (
                <div className="flex flex-col items-center pointer-events-none">
                    <Upload className="h-8 w-8 mx-auto mb-2 text-muted-foreground" />
                    <p className="text-sm font-medium">
                        {intl.formatMessage({
                            id: "pdfFileUpload.placeholder",
                            defaultMessage: "Click to upload PDF",
                        })}
                    </p>
                    <p className="text-xs text-muted-foreground mt-1">
                        {intl.formatMessage({
                            id: "pdfFileUpload.dragDrop",
                            defaultMessage: "Or drag and drop PDF here",
                        })}
                    </p>
                </div>
            );
        }

        if (Array.isArray(file)) {
            return (
                <div className="flex flex-col gap-2 w-full max-w-md mx-auto">
                    <div className="flex items-center justify-center gap-2 mb-2 text-primary font-medium pointer-events-none">
                        <Upload className="h-4 w-4" />
                        <span>
                            {intl.formatMessage(
                                {
                                    id: "pdfFileUpload.multipleFiles",
                                    defaultMessage: "{count} files selected",
                                },
                                { count: file.length }
                            )}
                        </span>
                    </div>
                    <p className="text-sm text-muted-foreground text-center">
                        {intl.formatMessage({
                            id: "pdfFileUpload.multipleFilesDescription",
                            defaultMessage:
                                "Drag and drop your files here, or click to browse",
                        })}
                    </p>
                    <div className="flex flex-col gap-2">
                        {file.map((f, i) => (
                            <div
                                key={i}
                                className="flex items-center gap-3 p-2 bg-muted/50 rounded-md text-left group relative pr-8 cursor-default"
                            >
                                <div className="p-2 bg-background rounded-md shadow-sm">
                                    <FileIcon className="h-4 w-4 text-primary" />
                                </div>
                                <div className="flex flex-col min-w-0 flex-1">
                                    <span className="text-sm font-medium truncate">
                                        {f.name}
                                    </span>
                                    <span className="text-xs text-muted-foreground">
                                        {formatFileSize(f.size)}
                                    </span>
                                </div>
                                <Button
                                    type="button"
                                    onClick={(e) => handleRemove(e, i)}
                                    variant="destructive"
                                    size="icon-sm"
                                    className="absolute right-2 top-1/2 -translate-y-1/2 p-1 hover:bg-destructive/10 hover:text-destructive rounded-full transition-colors cursor-pointer"
                                >
                                    <X className="h-4 w-4" />
                                </Button>
                            </div>
                        ))}
                    </div>
                </div>
            );
        }

        return (
            <div className="flex flex-col items-center relative group cursor-default">
                <FileIcon className="h-8 w-8 mx-auto mb-2 text-primary" />
                <p className="text-sm font-medium text-primary">{file.name}</p>
                <p className="text-xs text-muted-foreground mt-1">
                    {formatFileSize(file.size)}
                </p>
                <Button
                    onClick={(e) => handleRemove(e)}
                    type="button"
                    variant="destructive"
                    size="icon-sm"
                    className="mt-2 hover:bg-destructive/10 hover:text-destructive rounded-full transition-colors cursor-pointer"
                    title={intl.formatMessage({
                        id: "pdfFileUpload.removeFile",
                        defaultMessage: "Remove file",
                    })}
                >
                    <X className="h-4 w-4" />
                </Button>
            </div>
        );
    };

    return (
        <div
            onClick={handleClick}
            onDragOver={handleDrag}
            onDragEnter={handleDragIn}
            onDragLeave={handleDragOut}
            onDrop={handleDrop}
            className={cn(
                `border-2 relative border-dashed rounded-lg p-8 text-center transition-colors cursor-pointer ${
                    isDragging
                        ? "border-primary bg-primary/5"
                        : "border-border hover:border-primary/50"
                }`,
                className
            )}
        >
            <Input
                ref={inputRef}
                type="file"
                accept=".pdf"
                className="hidden"
                id="pdf-upload"
                multiple={multiple}
                onChange={(e) => {
                    const files = e.target.files;
                    if (files && files.length > 0) {
                        if (multiple) {
                            onFileChange(Array.from(files));
                        } else {
                            onFileChange(files[0]);
                        }
                    } else if (!multiple) {
                        onFileChange(null);
                    }
                    e.target.value = "";
                }}
            />
            <div className="w-full h-full flex flex-col justify-center items-center">
                {displayFile()}
            </div>
        </div>
    );
};
