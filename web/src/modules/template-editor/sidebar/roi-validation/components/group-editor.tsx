import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";
import type { RoiValidationConditionGroupType } from "@/modules/rois/validation/schema";
import { ConditionNodeEditor } from "@/modules/template-editor/sidebar/roi-validation/components/condition-node-editor";
import type {
    NodeUpdater,
    RoiValidationRuleDefinition,
} from "@/modules/template-editor/sidebar/roi-validation/components/types";
import { Plus } from "lucide-react";
import { useIntl } from "react-intl";

type GroupEditorProps = {
    node: RoiValidationConditionGroupType;
    path: number[];
    depth: number;
    editable: boolean;
    canRemove: boolean;
    rules: RoiValidationRuleDefinition[];
    onUpdate: NodeUpdater;
    onAddRule: (path: number[]) => void;
    onAddGroup: (path: number[]) => void;
    onRemoveNode: (path: number[]) => void;
};

const DEPTH_BORDER_CLASSES = [
    "border",
    "border-sky-400",
    "border-emerald-400",
    "border-amber-400",
    "border-rose-400",
] as const;

const MAX_DEPTH = 4;

export const GroupEditor = ({
    node,
    path,
    depth,
    rules,
    editable,
    canRemove,
    onUpdate,
    onAddRule,
    onAddGroup,
    onRemoveNode,
}: GroupEditorProps) => {
    const intl = useIntl();
    const borderClass =
        DEPTH_BORDER_CLASSES[depth % DEPTH_BORDER_CLASSES.length];

    return (
        <div className={`space-y-2 rounded-md border p-3 mb-10 ${borderClass}`}>
            <div className="flex items-center justify-between gap-2">
                <div className="flex items-center gap-4">
                    <Label className="text-s">
                        {intl.formatMessage({
                            id: "template-editor.roi-validation.group",
                            defaultMessage: "Group operator",
                        })}
                    </Label>
                    <Select
                        value={node.operator}
                        disabled={!editable}
                        onValueChange={(value) =>
                            onUpdate(path, (currentNode) =>
                                currentNode.type === "group"
                                    ? {
                                          ...currentNode,
                                          operator: value as "AND" | "OR",
                                      }
                                    : currentNode,
                            )
                        }
                    >
                        <SelectTrigger
                            className="h-8 rounded-md border px-2 text-sm"
                            disabled={!editable}
                        >
                            <SelectValue placeholder="Select operator" />
                        </SelectTrigger>
                        <SelectContent>
                            <SelectItem value="AND">
                                {intl.formatMessage({
                                    id: "template-editor.roi-validation.operator.and",
                                    defaultMessage: "AND",
                                })}
                            </SelectItem>
                            <SelectItem value="OR">
                                {intl.formatMessage({
                                    id: "template-editor.roi-validation.operator.or",
                                    defaultMessage: "OR",
                                })}
                            </SelectItem>
                        </SelectContent>
                    </Select>
                </div>
                {canRemove && editable ? (
                    <Button
                        type="button"
                        variant="outline"
                        onClick={() => onRemoveNode(path)}
                    >
                        {intl.formatMessage({
                            id: "template-editor.roi-validation.remove-group",
                            defaultMessage: "Remove",
                        })}
                    </Button>
                ) : null}
            </div>

            <div className="space-y-2">
                {node.children.map((child, childIndex) => (
                    <ConditionNodeEditor
                        key={`${child.type}-${childIndex}`}
                        node={child}
                        path={[...path, childIndex]}
                        depth={depth + 1}
                        editable={editable}
                        canRemove={node.children.length > 1}
                        rules={rules}
                        onUpdate={onUpdate}
                        onAddRule={onAddRule}
                        onAddGroup={onAddGroup}
                        onRemoveNode={onRemoveNode}
                    />
                ))}
            </div>

            {editable ? (
                <div className="flex items-center gap-2">
                    <Button
                        type="button"
                        size="sm"
                        variant="outline"
                        disabled={rules.length === 0}
                        onClick={() => onAddRule(path)}
                    >
                        <Plus />
                        {intl.formatMessage({
                            id: "template-editor.roi-validation.add-rule",
                            defaultMessage: "Add rule",
                        })}
                    </Button>
                    <Button
                        type="button"
                        size="sm"
                        variant="outline"
                        disabled={rules.length === 0 || depth >= MAX_DEPTH}
                        onClick={() => onAddGroup(path)}
                    >
                        <Plus />
                        {intl.formatMessage({
                            id: "template-editor.roi-validation.add-group",
                            defaultMessage: "Add group",
                        })}
                    </Button>
                </div>
            ) : null}
        </div>
    );
};
