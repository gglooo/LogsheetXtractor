import { FormInput } from "@/components/form/form-input";
import { FormSelect } from "@/components/form/form-select";
import { FormAutoSubmit } from "@/components/ui/form";
import { roiTypeSelectOptions } from "@/modules/rois/schema";
import { useSelectedRois } from "@/modules/template-editor/hooks/use-selected-rois";
import { useTemplateEditor } from "@/modules/template-editor/hooks/use-template-editor";
import type { EditRoiFormValues } from "@/modules/template-editor/sidebar/schema";
import { VARIABLE_NAME_INPUT_ID } from "@/modules/template-editor/sidebar/selected-roi";
import { Info } from "lucide-react";
import { useIntl } from "react-intl";

export const MultipleSelectedRois = ({
    selectedRoisCount,
}: {
    selectedRoisCount: number;
}) => {
    const intl = useIntl();

    const { setRois } = useTemplateEditor();
    const { isSelectedRoi } = useSelectedRois();

    const handleSubmit = async (values: EditRoiFormValues) => {
        setRois((prevRois) => {
            let increment = 1;

            return prevRois.map((roi) => {
                if (isSelectedRoi(roi.id ?? "")) {
                    const newVariableName = `${values.variableName}_${increment}`;
                    increment += 1;
                    return {
                        ...roi,
                        variableName: newVariableName,
                        type: values.type,
                    };
                }
                return roi;
            });
        });
    };

    return (
        <div className="flex flex-col gap-6 p-1">
            <header className="flex items-center justify-between">
                <h3 className="font-semibold text-sm">
                    {intl.formatMessage({
                        id: "template-editor.sidebar.multiple-select-rois.title",
                        defaultMessage: "Bulk edit",
                    })}
                </h3>
                <span className="bg-primary/10 text-primary text-xs px-2 py-0.5 rounded-full font-bold">
                    {intl.formatMessage(
                        {
                            id: "template-editor.sidebar.multiple-select-rois.selectedCount",
                            defaultMessage: "{count}",
                        },
                        { count: selectedRoisCount }
                    )}
                </span>
            </header>

            <section className="space-y-4">
                <div className="grid gap-4">
                    <div className="space-y-2">
                        <FormInput
                            name="variableName"
                            id={VARIABLE_NAME_INPUT_ID}
                            label={intl.formatMessage({
                                id: "template-editor.sidebar.multiple-select-rois.baseVariableName",
                                defaultMessage: "Base variable name",
                            })}
                            autoFocus
                            onFocus={(e) => e.currentTarget.select()}
                        />
                        <div className="flex items-center gap-2 text-muted-foreground">
                            <Info size={12} />
                            <span className="text-[11px]">
                                {intl.formatMessage({
                                    id: "template-editor.sidebar.multiple-select-rois.variableNameInfo",
                                    defaultMessage:
                                        "Auto-increments: prefix_1, prefix_2...",
                                })}
                            </span>
                        </div>
                    </div>

                    <FormSelect
                        name="type"
                        label={intl.formatMessage({
                            id: "type",
                            defaultMessage: "Type",
                        })}
                        options={roiTypeSelectOptions}
                    />
                </div>
            </section>

            <FormAutoSubmit onSubmit={handleSubmit} />
        </div>
    );
};
