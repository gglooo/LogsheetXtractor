import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";
import type { RoiType } from "@/modules/rois/schema";
import { usePredefinedRoiValidationConditions } from "@/modules/rois/validation/api";
import type { PredefinedRoiValidationConditionType } from "@/modules/rois/validation/schema";
import { usePresetLabel } from "@/modules/template-editor/sidebar/roi-validation/hooks/use-preset-label";
import { cloneValidationCondition } from "@/modules/template-editor/sidebar/roi-validation/utils";
import { useIntl } from "react-intl";

type SelectPresetProps = {
    roiType: RoiType["type"];
    editable: boolean;
    onSelect: (condition: RoiType["validationCondition"]) => void;
};

export const SelectPreset = ({ roiType, editable, onSelect }: SelectPresetProps) => {
    const intl = useIntl();
    const getPresetLabel = usePresetLabel();
    const predefinedConditionsQuery = usePredefinedRoiValidationConditions(roiType);

    const handleApplyPredefinedCondition = (predefinedConditionId: string) => {
        if (!editable || !predefinedConditionsQuery.data) {
            return;
        }

        const selectedCondition = predefinedConditionsQuery.data.find(
            (item) => item.id === predefinedConditionId,
        );
        if (!selectedCondition) {
            return;
        }

        const copiedCondition = cloneValidationCondition<
            PredefinedRoiValidationConditionType["condition"]
        >(selectedCondition.condition);
        onSelect(copiedCondition);
    };

    return (
        <div className="grid gap-1">
            <span className="text-xs font-medium">
                {intl.formatMessage({
                    id: "template-editor.roi-validation.predefined.label",
                    defaultMessage: "Preset condition",
                })}
            </span>
            <Select
                value=""
                disabled={
                    !editable ||
                    predefinedConditionsQuery.isLoading ||
                    !predefinedConditionsQuery.data ||
                    predefinedConditionsQuery.data.length === 0
                }
                onValueChange={handleApplyPredefinedCondition}
            >
                <SelectTrigger className="h-8 rounded-md border px-2 text-sm w-full">
                    <SelectValue
                        placeholder={intl.formatMessage({
                            id: "template-editor.roi-validation.predefined.placeholder",
                            defaultMessage: "Select a preset...",
                        })}
                    />
                </SelectTrigger>
                <SelectContent>
                    {(predefinedConditionsQuery.data ?? []).map((item) => {
                        const translatedLabel = getPresetLabel(
                            item.code,
                            item.label,
                        );

                        return (
                            <SelectItem key={item.id} value={item.id}>
                                {translatedLabel}
                            </SelectItem>
                        );
                    })}
                </SelectContent>
            </Select>
        </div>
    );
};
