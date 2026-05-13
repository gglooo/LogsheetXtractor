import type { RoiValidationConditionGroupType } from "@/modules/rois/validation/schema";
import {
    cloneValidationCondition,
    removeNodeAtPath,
    updateNodeAtPath,
} from "@/modules/template-editor/sidebar/roi-validation/utils";
import { describe, expect, it } from "vitest";

const nestedCondition = (): RoiValidationConditionGroupType => ({
    type: "group",
    operator: "OR",
    children: [
        {
            type: "group",
            operator: "AND",
            children: [
                {
                    type: "rule",
                    ruleType: "text.prefix",
                    params: { value: "ABC" },
                },
            ],
        },
        {
            type: "rule",
            ruleType: "common.requiredNonEmpty",
            params: {},
        },
    ],
});

describe("roi validation condition tree utilities", () => {
    it("updates nested rule groups by path", () => {
        const updated = updateNodeAtPath(nestedCondition(), [0, 0], (node) =>
            node.type === "rule"
                ? {
                      ...node,
                      ruleType: "text.suffix",
                      params: { value: "XYZ" },
                  }
                : node,
        );

        const nestedGroup = updated.children[0];
        expect(nestedGroup.type).toBe("group");
        expect(nestedGroup.type === "group" && nestedGroup.children[0]).toEqual(
            {
                type: "rule",
                ruleType: "text.suffix",
                params: { value: "XYZ" },
            },
        );
    });

    it("ignores invalid selection paths without changing the tree", () => {
        const condition = nestedCondition();

        expect(
            updateNodeAtPath(condition, [2, 0], () => ({
                type: "rule",
                ruleType: "text.suffix",
                params: {},
            })),
        ).toBe(condition);
    });

    it("removes a child but keeps groups non-empty", () => {
        const updated = removeNodeAtPath(nestedCondition(), [1]);
        expect(updated.children).toHaveLength(1);

        const unchanged = removeNodeAtPath(updated, [0]);
        expect(unchanged.children).toHaveLength(1);
    });

    it("clones preset validation configs before applying them to ROIs", () => {
        const preset = nestedCondition();
        const clone = cloneValidationCondition(preset);

        expect(clone).toEqual(preset);
        expect(clone).not.toBe(preset);
        expect(clone.children[0]).not.toBe(preset.children[0]);
    });
});
