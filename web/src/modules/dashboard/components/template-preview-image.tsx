import { FilesIcon } from "lucide-react";

interface TemplatePreviewImageProps {
    templateId: string;
    templateName: string;
    fileId: string | null;
}

export const TemplatePreviewImage = ({
    templateId,
    templateName,
    fileId,
}: TemplatePreviewImageProps) => {
    if (!fileId) {
        return (
            <div className="w-full h-40 bg-muted border-b flex items-center justify-center">
                <FilesIcon className="h-12 w-12 text-muted-foreground opacity-20" />
            </div>
        );
    }

    return (
        <div className="w-full h-40 bg-muted border-b relative group">
            <img
                src={`/api/templates/${templateId}/preview`}
                alt={`${templateName} preview`}
                className="w-full h-full object-cover object-top"
                onError={(e) => {
                    e.currentTarget.style.display = "none";
                    e.currentTarget.parentElement?.classList.add(
                        "flex",
                        "items-center",
                        "justify-center",
                    );
                    const icon = document.createElement("div");
                    icon.innerHTML =
                        '<svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="text-muted-foreground opacity-20"><path d="M14.5 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V7.5L14.5 2z"/><polyline points="14 2 14 8 20 8"/></svg>';
                    e.currentTarget.parentElement?.appendChild(
                        icon.firstChild as Node,
                    );
                }}
            />
        </div>
    );
};
