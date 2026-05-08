import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";
import type { RoiValidationConditionRuleType } from "@/modules/rois/validation/schema";
import { toRecord } from "@/modules/template-editor/sidebar/roi-validation/components/rule-catalog-utils";
import { RuleParamsEditor } from "@/modules/template-editor/sidebar/roi-validation/components/rule-params-editor";
import type {
    NodeUpdater,
    RoiValidationRuleDefinition,
} from "@/modules/template-editor/sidebar/roi-validation/components/types";
import { Trash2 } from "lucide-react";
import { useIntl } from "react-intl";

type RuleEditorProps = {
    node: RoiValidationConditionRuleType;
    path: number[];
    editable: boolean;
    canRemove: boolean;
    rules: RoiValidationRuleDefinition[];
    onUpdate: NodeUpdater;
    onRemoveNode: (path: number[]) => void;
};

export const RuleEditor = ({
    node,
    path,
    editable,
    canRemove,
    rules,
    onUpdate,
    onRemoveNode,
}: RuleEditorProps) => {
    const intl = useIntl();

    return (
        <div className="rounded-md border p-3 space-y-3">
            <div className="flex items-center gap-2">
                <Label className="text-xs shrink-0">
                    {intl.formatMessage({
                        id: "template-editor.roi-validation.rule",
                        defaultMessage: "Rule",
                    })}
                </Label>
                <Select
                    value={node.ruleType}
                    disabled={!editable}
                    onValueChange={(value) =>
                        onUpdate(path, (currentNode) => {
                            if (currentNode.type !== "rule") {
                                return currentNode;
                            }

                            const selectedRule = rules.find(
                                (rule) => rule.ruleType === value,
                            );
                            if (!selectedRule) {
                                return currentNode;
                            }

                            return {
                                ...currentNode,
                                ruleType: selectedRule.ruleType,
                                params: toRecord(selectedRule.defaultParams),
                            };
                        })
                    }
                >
                    <SelectTrigger
                        className="h-8 rounded-md border px-2 text-sm w-full"
                        disabled={!editable}
                    >
                        <SelectValue placeholder="Select rule" />
                    </SelectTrigger>
                    <SelectContent>
                        {rules.map((rule) => (
                            <SelectItem
                                key={rule.ruleType}
                                value={rule.ruleType}
                            >
                                {rule.label}
                            </SelectItem>
                        ))}
                    </SelectContent>
                </Select>
                {canRemove && editable ? (
                    <Button
                        type="button"
                        size="icon"
                        variant="outline"
                        onClick={() => onRemoveNode(path)}
                    >
                        <Trash2 />
                    </Button>
                ) : null}
            </div>
            <RuleParamsEditor
                rule={node}
                editable={editable}
                onChange={(params) =>
                    onUpdate(path, (currentNode) =>
                        currentNode.type === "rule"
                            ? {
                                  ...currentNode,
                                  params,
                              }
                            : currentNode,
                    )
                }
            />
        </div>
    );
};
