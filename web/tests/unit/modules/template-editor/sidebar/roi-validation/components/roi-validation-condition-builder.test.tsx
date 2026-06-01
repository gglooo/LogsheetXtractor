import { RoiValidationConditionBuilder } from "@/modules/template-editor/sidebar/roi-validation/components/roi-validation-condition-builder";
import type { RoiValidationRuleCatalogType } from "@/modules/rois/validation/schema";
import { fireEvent, render, screen } from "@testing-library/react";
import type React from "react";
import { IntlProvider } from "react-intl";
import { beforeEach, describe, expect, it, vi } from "vitest";

const catalogQueryMock = vi.hoisted(() => vi.fn());

vi.mock("@/modules/rois/validation/api", () => ({
    useRoiValidationRuleCatalog: () => catalogQueryMock(),
}));

vi.mock(
    "@/modules/template-editor/sidebar/roi-validation/components/condition-node-editor",
    () => ({
        ConditionNodeEditor: ({
            onAddRule,
            onAddGroup,
            onRemoveNode,
            onUpdate,
        }: {
            onAddRule: (path: number[]) => void;
            onAddGroup: (path: number[]) => void;
            onRemoveNode: (path: number[]) => void;
            onUpdate: (
                path: number[],
                updater: (node: { type: "group"; operator: "AND" | "OR"; children: [] }) => unknown,
            ) => void;
        }) => (
            <div data-testid="condition-node-editor">
                <button onClick={() => onAddRule([])}>add child rule</button>
                <button onClick={() => onAddGroup([])}>add child group</button>
                <button onClick={() => onRemoveNode([1])}>remove child</button>
                <button
                    onClick={() =>
                        onUpdate([], (node) => ({
                            ...node,
                            operator: "OR",
                        }))
                    }
                >
                    update group
                </button>
            </div>
        ),
    }),
);

const Wrapper = ({ children }: { children: React.ReactNode }) => (
    <IntlProvider locale="en">{children}</IntlProvider>
);

const catalog: RoiValidationRuleCatalogType = {
    version: "1",
    roiTypes: [
        {
            roiType: "Handwritten",
            rules: [
                {
                    ruleType: "required",
                    label: "Required",
                    description: "Required",
                    defaultParams: { minLength: 1 },
                },
            ],
        },
    ],
};

const condition = {
    type: "group" as const,
    operator: "AND" as const,
    children: [
        {
            type: "rule" as const,
            ruleType: "required",
            params: { minLength: 1 },
        },
    ],
};

describe("RoiValidationConditionBuilder", () => {
    beforeEach(() => {
        catalogQueryMock.mockReturnValue({
            data: catalog,
            isLoading: false,
            isError: false,
        });
    });

    it("shows loading state while the rule catalog is loading", () => {
        catalogQueryMock.mockReturnValue({
            data: undefined,
            isLoading: true,
            isError: false,
        });

        render(
            <RoiValidationConditionBuilder
                roiType="Handwritten"
                validationCondition={null}
                editable
                onChange={vi.fn()}
            />,
            { wrapper: Wrapper },
        );

        expect(screen.getByText("Loading validation rules...")).toBeInTheDocument();
    });

    it("enables validation with the first available rule", () => {
        const onChange = vi.fn();

        render(
            <RoiValidationConditionBuilder
                roiType="Handwritten"
                validationCondition={null}
                editable
                onChange={onChange}
            />,
            { wrapper: Wrapper },
        );

        fireEvent.click(screen.getByRole("button", { name: /add conditions/i }));

        expect(onChange).toHaveBeenCalledWith(condition);
    });

    it("clears and updates an existing condition", () => {
        const onChange = vi.fn();

        render(
            <RoiValidationConditionBuilder
                roiType="Handwritten"
                validationCondition={condition}
                editable
                onChange={onChange}
            />,
            { wrapper: Wrapper },
        );

        fireEvent.click(screen.getByRole("button", { name: /clear/i }));
        expect(onChange).toHaveBeenCalledWith(null);

        fireEvent.click(screen.getByRole("button", { name: /add child rule/i }));
        expect(onChange).toHaveBeenLastCalledWith({
            ...condition,
            children: [...condition.children, condition.children[0]],
        });

        fireEvent.click(screen.getByRole("button", { name: /add child group/i }));
        expect(onChange).toHaveBeenLastCalledWith({
            ...condition,
            children: [...condition.children, condition],
        });

        fireEvent.click(screen.getByRole("button", { name: /update group/i }));
        expect(onChange).toHaveBeenLastCalledWith({
            ...condition,
            operator: "OR",
        });
    });

    it("does not expose edit controls when read-only", () => {
        render(
            <RoiValidationConditionBuilder
                roiType="Handwritten"
                validationCondition={condition}
                editable={false}
                onChange={vi.fn()}
            />,
            { wrapper: Wrapper },
        );

        expect(screen.getByText("Read-only")).toBeInTheDocument();
        expect(screen.queryByRole("button", { name: /clear/i })).not.toBeInTheDocument();
    });
});
