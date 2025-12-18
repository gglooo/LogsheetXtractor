import { Upload } from "lucide-react";
import { useState } from "react";

export const PdfFileUpload = ({
    file,
    onFileChange,
}: {
    file: File | null;
    onFileChange: (file: File | null) => void;
}) => {
    const [isDragging, setIsDragging] = useState(false);

    const handleDrag = (e: React.DragEvent) => {
        e.preventDefault();
        e.stopPropagation();
    };

    const handleDrop = (e: React.DragEvent) => {
        handleDrag(e);
        setIsDragging(false);

        const droppedFiles = e.dataTransfer.files;
        if (droppedFiles && droppedFiles.length > 0) {
            onFileChange(droppedFiles[0]);
        }
    };

    return (
        <div className="space-y-2">
            <div
                className={`border-2 relative border-dashed rounded-lg p-8 text-center transition-colors cursor-pointer ${
                    isDragging
                        ? "border-primary bg-primary/5"
                        : "border-border hover:border-primary/50"
                }`}
            >
                <input
                    type="file"
                    accept=".pdf"
                    className="hidden"
                    id="pdf-upload"
                    onChange={(e) => onFileChange(e.target.files?.[0] || null)}
                />
                <label
                    htmlFor="pdf-upload"
                    className="cursor-pointer w-full h-full block"
                >
                    <Upload className="h-8 w-8 mx-auto mb-2 text-muted-foreground" />
                    <p className="text-sm font-medium">
                        {file ? file.name : "Click to upload PDF"}
                    </p>
                    <p className="text-xs text-muted-foreground mt-1">
                        or drag and drop
                    </p>
                    <div
                        onDragOver={handleDrag}
                        onDragEnter={(e) => {
                            handleDrag(e);
                            setIsDragging(true);
                        }}
                        onDragLeave={(e) => {
                            handleDrag(e);
                            setIsDragging(false);
                        }}
                        onDrop={handleDrop}
                        className="absolute top-0 left-0 w-full h-full"
                    />
                </label>
            </div>
        </div>
    );
};
