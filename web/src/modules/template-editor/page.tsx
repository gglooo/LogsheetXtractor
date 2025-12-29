import { SidebarInset, SidebarProvider } from "@/components/ui/sidebar";
import { Spinner } from "@/components/ui/spinner";
import { DrawablePdfViewer } from "@/modules/pdf/components/drawable-pdf-viewer";
import { PdfWrapper } from "@/modules/pdf/components/pdf-wrapper";
import { EditorNavbar } from "@/modules/template-editor/components/navbar";
import { SelectedRoisProvider } from "@/modules/template-editor/context/selected-rois-context";
import { TemplateEditorProvider } from "@/modules/template-editor/context/template-editor-context";
import { useTemplateEditor } from "@/modules/template-editor/hooks/use-template-editor";
import { EditorSidebar } from "@/modules/template-editor/sidebar/sidebar";
import { useTemplate } from "@/modules/templates/api";
import { useParams } from "react-router-dom";

export const TemplateEditorPage = () => {
    const { id } = useParams<{ id: string }>();

    const templateQuery = useTemplate(id!);

    if (templateQuery.isLoading) {
        return (
            <div className="flex h-screen items-center justify-center">
                <Spinner />
            </div>
        );
    }

    if (templateQuery.isError || !templateQuery.data) {
        return <div className="p-4">Failed to load template.</div>;
    }

    return (
        <TemplateEditorProvider template={templateQuery.data}>
            <SelectedRoisProvider>
                <SidebarProvider>
                    <TemplateEditorContent />
                </SidebarProvider>
            </SelectedRoisProvider>
        </TemplateEditorProvider>
    );
};

export const TemplateEditorContent = () => {
    const { mode, template } = useTemplateEditor();

    if (!template) {
        return <div>No template loaded.</div>;
    }

    return (
        <SidebarProvider>
            <div className="flex h-screen w-full overflow-hidden">
                <EditorSidebar />
                <SidebarInset className="flex flex-col flex-1 overflow-hidden">
                    <EditorNavbar />

                    <main className="flex-1 overflow-auto p-4">
                        <h1 className="text-2xl font-bold mb-4">
                            {template.name}
                        </h1>

                        {template ? (
                            <div
                                className={
                                    mode === "draw" ? "cursor-crosshair" : ""
                                }
                            >
                                {template.file?.id ? (
                                    <PdfWrapper>
                                        <DrawablePdfViewer
                                            fileId={template.file.id}
                                            template={template}
                                        />
                                    </PdfWrapper>
                                ) : null}
                            </div>
                        ) : (
                            <Spinner />
                        )}
                    </main>
                </SidebarInset>
            </div>
        </SidebarProvider>
    );
};
