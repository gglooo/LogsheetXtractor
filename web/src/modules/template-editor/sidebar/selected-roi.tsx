import { FormInput } from "@/components/form/form-input";
import { FormSelect } from "@/components/form/form-select";
import { Form, FormAutoSubmit } from "@/components/ui/form";
import { SidebarGroup } from "@/components/ui/sidebar";
import { roiTypeSelectOptions, type RoiType } from "@/modules/rois/schema";
import { useSelectedRois } from "@/modules/template-editor/hooks/use-selected-rois";
import { useTemplateEditor } from "@/modules/template-editor/hooks/use-template-editor";
import { MultipleSelectedRois } from "@/modules/template-editor/sidebar/multiple-selected-rois";
import { RoiValidationConditionSheet } from "@/modules/template-editor/sidebar/roi-validation/roi-validation-condition-sheet";
import {
    editRoiSchema,
    type EditRoiFormValues,
} from "@/modules/template-editor/sidebar/schema";
import { useIntl } from "react-intl";

export const VARIABLE_NAME_INPUT_ID = "variableNameInput";

const SelectedRoiContent = ({
    selectedRois,
    editable,
}: {
    selectedRois: RoiType[];
    editable: boolean;
}) => {
    const { setRois, roiInputRef } = useTemplateEditor();
    const intl = useIntl();

    if (selectedRois.length > 1) {
        return <MultipleSelectedRois selectedRoisCount={selectedRois.length} />;
    }
    if (selectedRois.length === 0) {
        return (
            <div className="text-center">
                {intl.formatMessage({
                    id: "selectedRoi.noRoiSelected",
                    defaultMessage: "No ROI selected",
                })}
            </div>
        );
    }

    const selectedRoi = selectedRois[0];

    const handleSubmit = async (values: EditRoiFormValues) => {
        setRois((prevRois) =>
            prevRois.map((roi) =>
                roi.id === selectedRoi.id
                    ? {
                          ...roi,
                          ...values,
                          validationCondition:
                              values.type !== selectedRoi.type
                                  ? null
                                  : roi.validationCondition,
                      }
                    : roi,
            ),
        );
    };

    return (
        <div className="flex flex-col gap-4">
            <FormInput
                name="variableName"
                id={VARIABLE_NAME_INPUT_ID}
                label={intl.formatMessage({
                    id: "selectedRoi.variableName",
                    defaultMessage: "Variable name",
                })}
                defaultValue={selectedRoi.variableName}
                onFocus={(e) => e.currentTarget.select()}
                ref={roiInputRef}
                labelClassname="font-bold"
                disabled={!editable}
            />
            <FormSelect
                name="type"
                label={intl.formatMessage({
                    id: "selectedRoi.type",
                    defaultMessage: "Type",
                })}
                options={roiTypeSelectOptions}
                labelClassName="font-bold"
                disabled={!editable}
            />
            <RoiValidationConditionSheet
                selectedRoi={selectedRoi}
                editable={editable}
                onChangeValidationCondition={(validationCondition) =>
                    setRois((prevRois) =>
                        prevRois.map((roi) =>
                            roi.id === selectedRoi.id
                                ? {
                                      ...roi,
                                      validationCondition,
                                  }
                                : roi,
                        ),
                    )
                }
            />
            <FormAutoSubmit onSubmit={handleSubmit} />
        </div>
    );
};

export const SelectedRoiSidebarGroup = () => {
    const intl = useIntl();
    const { isSelectedRoi } = useSelectedRois();
    const { rois, template } = useTemplateEditor();

    const selectedRois = rois.filter((roi) => isSelectedRoi(roi.id ?? ""));

    const selectedRoi = selectedRois[0];
    const key = selectedRoi
        ? `${selectedRoi.id}-${selectedRoi.type}`
        : "no-selection";

    return (
        <SidebarGroup
            title={intl.formatMessage({
                id: "selectedRoi.sidebarTitle",
                defaultMessage: "Selected ROI",
            })}
            className="flex flex-col gap-2"
        >
            <Form
                key={key}
                schema={editRoiSchema}
                defaultValues={{
                    variableName: selectedRoi?.variableName || "",
                    type: selectedRoi?.type || "Handwritten",
                }}
            >
                <SelectedRoiContent
                    selectedRois={selectedRois}
                    editable={!!template?.isEditable}
                />
            </Form>
        </SidebarGroup>
    );
};
