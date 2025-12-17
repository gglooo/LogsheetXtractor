import { Label } from "@/components/ui/label";
import { Upload } from "lucide-react";

export const PdfFileUpload = ({
    label,
    file,
    onFileChange,
}: {
    label: string;
    file: File | null;
    onFileChange: (file: File | null) => void;
}) => {
    return (
        <div className="space-y-2">
            <Label>{label}</Label>
            <div className="border-2 border-dashed border-border rounded-lg p-8 text-center hover:border-primary/50 transition-colors cursor-pointer">
                <input
                    type="file"
                    accept=".pdf"
                    className="hidden"
                    id="pdf-upload"
                    onChange={(e) => onFileChange(e.target.files?.[0] || null)}
                />
                <label htmlFor="pdf-upload" className="cursor-pointer">
                    <Upload className="h-8 w-8 mx-auto mb-2 text-muted-foreground" />
                    <p className="text-sm font-medium">
                        {file ? file.name : "Click to upload PDF"}
                    </p>
                    <p className="text-xs text-muted-foreground mt-1">
                        or drag and drop
                    </p>
                </label>
            </div>
        </div>
    );
};
