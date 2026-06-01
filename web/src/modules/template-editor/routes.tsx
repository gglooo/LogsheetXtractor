import { TemplateEditorPage } from "@/modules/template-editor/page";
import { Route, Routes } from "react-router-dom";

export const baseTemplateEditorPath = "/template-editor";

export const TemplateEditorRoutes = () => {
    return (
        <Routes>
            <Route path="/:id" element={<TemplateEditorPage />} />
        </Routes>
    );
};
