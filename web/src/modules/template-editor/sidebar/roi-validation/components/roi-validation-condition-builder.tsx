import { Button } from "@/components/ui/button";
import type { RoiType } from "@/modules/rois/schema";
import { useRoiValidationRuleCatalog } from "@/modules/rois/validation/api";
import type {
    RoiValidationConditionGroupType,
    RoiValidationConditionNodeType,
} from "@/modules/rois/validation/schema";
import {
    removeNodeAtPath,
    updateNodeAtPath,
} from "@/modules/template-editor/sidebar/roi-validation/utils";
import { Plus } from "lucide-react";
import { useMemo } from "react";
import { useIntl } from "react-intl";
import { ConditionNodeEditor } from "./condition-node-editor";
import {
    buildDefaultGroupNode,
    buildDefaultRuleNode,
    getRulesForRoiType,
} from "./rule-catalog-utils";

type RoiValidationConditionBuilderProps = {
    roiType: RoiType["type"];
    validationCondition: RoiType["validationCondition"];
    editable: boolean;
    onChange: (validationCondition: RoiType["validationCondition"]) => void;
};

export const RoiValidationConditionBuilder = ({
    roiType,
    validationCondition,
    editable,
    onChange,
}: RoiValidationConditionBuilderProps) => {
    const intl = useIntl();
    const catalogQuery = useRoiValidationRuleCatalog();

    const rules = useMemo(
        () =>
            catalogQuery.data
                ? getRulesForRoiType(catalogQuery.data, roiType)
                : [],
        [catalogQuery.data, roiType],
    );

    const updateCondition = (
        updater: (
            current: RoiValidationConditionGroupType,
        ) => RoiValidationConditionGroupType,
    ) => {
        if (!editable || !validationCondition) {
            return;
        }

        onChange(updater(validationCondition));
    };

    const handleEnable = () => {
        if (!editable || !catalogQuery.data) {
            return;
        }

        const defaultGroup = buildDefaultGroupNode(catalogQuery.data, roiType);
        if (!defaultGroup) {
            return;
        }

        onChange(defaultGroup);
    };

    const handleAddRule = (path: number[]) => {
        if (!editable || !catalogQuery.data) {
            return;
        }

        const newRule = buildDefaultRuleNode(catalogQuery.data, roiType);
        if (!newRule) {
            return;
        }

        updateCondition((current) =>
            updateNodeAtPath(current, path, (node) =>
                node.type === "group"
                    ? {
                          ...node,
                          children: [...node.children, newRule],
                      }
                    : node,
            ),
        );
    };

    const handleAddGroup = (path: number[]) => {
        if (!editable || !catalogQuery.data) {
            return;
        }

        const newGroup = buildDefaultGroupNode(catalogQuery.data, roiType);
        if (!newGroup) {
            return;
        }

        updateCondition((current) =>
            updateNodeAtPath(current, path, (node) =>
                node.type === "group"
                    ? {
                          ...node,
                          children: [...node.children, newGroup],
                      }
                    : node,
            ),
        );
    };

    if (catalogQuery.isLoading) {
        return (
            <div className="rounded-md border p-3 text-sm text-muted-foreground">
                {intl.formatMessage({
                    id: "template-editor.roi-validation.loading",
                    defaultMessage: "Loading validation rules...",
                })}
            </div>
        );
    }

    if (catalogQuery.isError || !catalogQuery.data) {
        return null;
    }

    return (
        <div className="space-y-3">
            <div className="flex items-center justify-between gap-2">
                <h4 className="text-sm font-semibold">
                    {intl.formatMessage({
                        id: "template-editor.roi-validation.title",
                        defaultMessage: "Validation conditions",
                    })}
                </h4>
                {editable && validationCondition ? (
                    <Button
                        type="button"
                        size="sm"
                        variant="outline"
                        onClick={() => onChange(null)}
                    >
                        {intl.formatMessage({
                            id: "template-editor.roi-validation.disable",
                            defaultMessage: "Clear",
                        })}
                    </Button>
                ) : editable ? (
                    <Button
                        type="button"
                        size="sm"
                        variant="outline"
                        disabled={rules.length === 0}
                        onClick={handleEnable}
                    >
                        <Plus />
                        {intl.formatMessage({
                            id: "template-editor.roi-validation.enable",
                            defaultMessage: "Add conditions",
                        })}
                    </Button>
                ) : null}
                {!editable && validationCondition ? (
                    <span className="text-xs text-muted-foreground">
                        {intl.formatMessage({
                            id: "template-editor.roi-validation.readonly",
                            defaultMessage: "Read-only",
                        })}
                    </span>
                ) : null}
            </div>

            {rules.length === 0 ? (
                <p className="text-xs text-muted-foreground">
                    {intl.formatMessage({
                        id: "template-editor.roi-validation.empty-catalog",
                        defaultMessage:
                            "No condition rules are available for this ROI type.",
                    })}
                </p>
            ) : null}

            {validationCondition ? (
                <ConditionNodeEditor
                    node={validationCondition as RoiValidationConditionNodeType}
                    path={[]}
                    depth={0}
                    editable={editable}
                    canRemove={false}
                    rules={rules}
                    onUpdate={(path, updater) =>
                        updateCondition((current) =>
                            updateNodeAtPath(current, path, updater),
                        )
                    }
                    onAddRule={handleAddRule}
                    onAddGroup={handleAddGroup}
                    onRemoveNode={(path) =>
                        updateCondition((current) =>
                            removeNodeAtPath(current, path),
                        )
                    }
                />
            ) : null}
        </div>
    );
};
