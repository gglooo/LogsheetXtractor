import { RuleParamsEditor } from "@/modules/template-editor/sidebar/roi-validation/components/rule-params-editor";
import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

const rule = {
    type: "rule" as const,
    ruleType: "sample",
    params: {
        enabled: true,
        threshold: 10,
        label: "ABC",
        allowedNumbers: [1, 2],
        allowedStrings: ["A", "B"],
        metadata: { mode: "strict" },
    },
};

describe("RuleParamsEditor", () => {
    it("renders an empty-state message when a rule has no parameters", () => {
        render(
            <RuleParamsEditor
                rule={{ type: "rule", ruleType: "empty", params: {} }}
                editable
                onChange={vi.fn()}
            />,
        );

        expect(screen.getByText("No parameters for this rule.")).toBeInTheDocument();
    });

    it("emits typed parameter updates", () => {
        const onChange = vi.fn();
        render(<RuleParamsEditor rule={rule} editable onChange={onChange} />);

        fireEvent.click(screen.getByLabelText("enabled"));
        expect(onChange).toHaveBeenLastCalledWith({
            ...rule.params,
            enabled: false,
        });

        fireEvent.change(screen.getByLabelText("threshold"), {
            target: { value: "12" },
        });
        expect(onChange).toHaveBeenLastCalledWith({
            ...rule.params,
            threshold: 12,
        });

        fireEvent.change(screen.getByLabelText("label"), {
            target: { value: "XYZ" },
        });
        expect(onChange).toHaveBeenLastCalledWith({
            ...rule.params,
            label: "XYZ",
        });

        fireEvent.change(screen.getByLabelText("allowedNumbers"), {
            target: { value: "3\nignored\n4" },
        });
        expect(onChange).toHaveBeenLastCalledWith({
            ...rule.params,
            allowedNumbers: [3, 4],
        });

        fireEvent.change(screen.getByLabelText("allowedStrings"), {
            target: { value: "C\nD" },
        });
        expect(onChange).toHaveBeenLastCalledWith({
            ...rule.params,
            allowedStrings: ["C", "D"],
        });

        fireEvent.change(screen.getByLabelText("metadata"), {
            target: { value: '{"mode":"loose"}' },
        });
        expect(onChange).toHaveBeenLastCalledWith({
            ...rule.params,
            metadata: { mode: "loose" },
        });
    });

    it("does not emit object updates until JSON input is valid", () => {
        const onChange = vi.fn();
        render(<RuleParamsEditor rule={rule} editable onChange={onChange} />);

        fireEvent.change(screen.getByLabelText("metadata"), {
            target: { value: "{" },
        });

        expect(onChange).not.toHaveBeenCalled();
    });
});
