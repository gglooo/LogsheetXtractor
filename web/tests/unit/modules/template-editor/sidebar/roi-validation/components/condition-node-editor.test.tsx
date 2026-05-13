import { ConditionNodeEditor } from "@/modules/template-editor/sidebar/roi-validation/components/condition-node-editor";
import type { RoiValidationRuleDefinition } from "@/modules/template-editor/sidebar/roi-validation/components/types";
import { render, screen } from "@testing-library/react";
import type React from "react";
import { IntlProvider } from "react-intl";
import { describe, expect, it, vi } from "vitest";

vi.mock(
    "@/modules/template-editor/sidebar/roi-validation/components/rule-editor",
    () => ({
        RuleEditor: ({ node }: { node: { ruleType: string } }) => (
            <div data-testid="rule-editor">{node.ruleType}</div>
        ),
    }),
);

vi.mock(
    "@/modules/template-editor/sidebar/roi-validation/components/group-editor",
    () => ({
        GroupEditor: ({ node }: { node: { operator: string } }) => (
            <div data-testid="group-editor">{node.operator}</div>
        ),
    }),
);

const Wrapper = ({ children }: { children: React.ReactNode }) => (
    <IntlProvider locale="en">{children}</IntlProvider>
);

const rules: RoiValidationRuleDefinition[] = [
    {
        ruleType: "required",
        label: "Required",
        description: "Required",
        defaultParams: {},
    },
];

const baseProps = {
    path: [],
    depth: 0,
    editable: true,
    canRemove: false,
    rules,
    onUpdate: vi.fn(),
    onAddRule: vi.fn(),
    onAddGroup: vi.fn(),
    onRemoveNode: vi.fn(),
};

describe("ConditionNodeEditor", () => {
    it("delegates rule nodes to RuleEditor", () => {
        render(
            <ConditionNodeEditor
                {...baseProps}
                node={{ type: "rule", ruleType: "required", params: {} }}
            />,
            { wrapper: Wrapper },
        );

        expect(screen.getByTestId("rule-editor")).toHaveTextContent("required");
    });

    it("delegates group nodes to GroupEditor", () => {
        render(
            <ConditionNodeEditor
                {...baseProps}
                node={{
                    type: "group",
                    operator: "OR",
                    children: [
                        { type: "rule", ruleType: "required", params: {} },
                    ],
                }}
            />,
            { wrapper: Wrapper },
        );

        expect(screen.getByTestId("group-editor")).toHaveTextContent("OR");
    });
});
