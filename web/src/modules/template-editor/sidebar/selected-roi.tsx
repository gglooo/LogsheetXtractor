import { FormInput } from "@/components/form/form-input";
import { FormSelect } from "@/components/form/form-select";
import { Form, FormAutoSubmit } from "@/components/ui/form";
import { SidebarGroup } from "@/components/ui/sidebar";
import {
    roiTypeSchema,
    roiTypeSelectOptions,
    type DetectedRoiType,
} from "@/modules/rois/schema";
import { useSelectedRois } from "@/modules/template-editor/hooks/use-selected-rois";
import { useTemplateEditor } from "@/modules/template-editor/hooks/use-template-editor";
import z from "zod";

const editRoiSchema = z.object({
    variableName: z.string().min(1),
    type: roiTypeSchema,
});

const SelectedRoiContent = ({
    selectedRois,
}: {
    selectedRois: DetectedRoiType[];
}) => {
    const { setRois } = useTemplateEditor();

    if (selectedRois.length > 1) {
        return (
            <div>{`Multiple ROIs selected (${selectedRois.length}). Please select a single ROI to edit.`}</div>
        );
    }
    if (selectedRois.length === 0) {
        return <div>No ROI selected.</div>;
    }

    const selectedRoi = selectedRois[0];

    const handleSubmit = async (values: z.infer<typeof editRoiSchema>) => {
        setRois((prevRois) =>
            prevRois.map((roi) =>
                roi.id === selectedRoi.id ? { ...roi, ...values } : roi
            )
        );
    };

    return (
        <div className="flex flex-col gap-4">
            <FormInput
                name="variableName"
                label="Variable Name"
                defaultValue={selectedRoi.variableName}
            />
            <FormSelect
                name="type"
                label="Type"
                options={roiTypeSelectOptions}
            />
            <FormAutoSubmit onSubmit={handleSubmit} />
        </div>
    );
};

export const SelectedRoiSidebarGroup = () => {
    const { selectedRoiIds } = useSelectedRois();
    const { rois } = useTemplateEditor();

    const selectedRois = rois.filter((roi) =>
        selectedRoiIds.includes(roi.id ?? "")
    );

    const selectedRoi = selectedRois[0];

    return (
        <SidebarGroup title="Selected ROI" className="flex flex-col gap-2">
            <Form
                key={selectedRoi?.id ?? "no-selection"}
                schema={editRoiSchema}
                defaultValues={{
                    variableName: selectedRoi?.variableName || "",
                    type: selectedRoi?.type || "Handwritten",
                }}
            >
                <SelectedRoiContent selectedRois={selectedRois} />
            </Form>
        </SidebarGroup>
    );
};
