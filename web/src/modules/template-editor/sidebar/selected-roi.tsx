import { FormInput } from "@/components/form/form-input";
import { FormSelect } from "@/components/form/form-select";
import { Form, FormAutoSubmit } from "@/components/ui/form";
import { SidebarGroup } from "@/components/ui/sidebar";
import { roiTypeSelectOptions, type RoiType } from "@/modules/rois/schema";
import { useSelectedRois } from "@/modules/template-editor/hooks/use-selected-rois";
import { useTemplateEditor } from "@/modules/template-editor/hooks/use-template-editor";
import { MultipleSelectedRois } from "@/modules/template-editor/sidebar/multiple-selected-rois";
import {
    editRoiSchema,
    type EditRoiFormValues,
} from "@/modules/template-editor/sidebar/schema";

export const VARIABLE_NAME_INPUT_ID = "variableNameInput";

const SelectedRoiContent = ({ selectedRois }: { selectedRois: RoiType[] }) => {
    const { setRois, roiInputRef } = useTemplateEditor();

    if (selectedRois.length > 1) {
        return <MultipleSelectedRois selectedRoisCount={selectedRois.length} />;
    }
    if (selectedRois.length === 0) {
        return <div>No ROI selected.</div>;
    }

    const selectedRoi = selectedRois[0];

    const handleSubmit = async (values: EditRoiFormValues) => {
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
                id={VARIABLE_NAME_INPUT_ID}
                label="Variable Name"
                defaultValue={selectedRoi.variableName}
                onFocus={(e) => e.currentTarget.select()}
                ref={roiInputRef}
                labelClassname="font-bold"
            />
            <FormSelect
                name="type"
                label="Type"
                options={roiTypeSelectOptions}
                labelClassName="font-bold"
            />
            <FormAutoSubmit onSubmit={handleSubmit} />
        </div>
    );
};

export const SelectedRoiSidebarGroup = () => {
    const { isSelectedRoi } = useSelectedRois();
    const { rois } = useTemplateEditor();

    const selectedRois = rois.filter((roi) => isSelectedRoi(roi.id ?? ""));

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
