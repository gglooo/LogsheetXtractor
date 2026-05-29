import type { RoiValidationConditionNodeType } from "@/modules/rois/validation/schema";
import { GroupEditor } from "@/modules/template-editor/sidebar/roi-validation/components/group-editor";
import { RuleEditor } from "@/modules/template-editor/sidebar/roi-validation/components/rule-editor";
import type { NodeUpdater, RoiValidationRuleDefinition } from "./types";

type ConditionNodeEditorProps = {
    node: RoiValidationConditionNodeType;
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

export const ConditionNodeEditor = ({
    node,
    path,
    depth,
    editable,
    canRemove,
    rules,
    onUpdate,
    onAddRule,
    onAddGroup,
    onRemoveNode,
}: ConditionNodeEditorProps) => {
    if (node.type === "rule") {
        return (
            <RuleEditor
                node={node}
                path={path}
                editable={editable}
                canRemove={canRemove}
                rules={rules}
                onUpdate={onUpdate}
                onRemoveNode={onRemoveNode}
            />
        );
    }

    return (
        <GroupEditor
            node={node}
            path={path}
            depth={depth}
            editable={editable}
            canRemove={canRemove}
            rules={rules}
            onUpdate={onUpdate}
            onAddRule={onAddRule}
            onAddGroup={onAddGroup}
            onRemoveNode={onRemoveNode}
        />
    );
};
