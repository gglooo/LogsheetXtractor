import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import type { RoiValidationConditionRuleType } from "@/modules/rois/validation/schema";

const parseArrayValues = (raw: string, current: unknown[]) => {
    const values = raw.split("\n");

    const shouldParseNumbers =
        current.length > 0 && typeof current[0] === "number";
    if (!shouldParseNumbers) {
        return values;
    }

    return values
        .map((item) => item.trim())
        .filter((item) => item.length > 0)
        .map((item) => Number(item))
        .filter((item) => !Number.isNaN(item));
};

type RuleParamsEditorProps = {
    rule: RoiValidationConditionRuleType;
    editable: boolean;
    onChange: (params: Record<string, unknown>) => void;
};

export const RuleParamsEditor = ({
    rule,
    editable,
    onChange,
}: RuleParamsEditorProps) => {
    const entries = Object.entries(rule.params);

    if (entries.length === 0) {
        return (
            <div className="text-xs text-muted-foreground">
                No parameters for this rule.
            </div>
        );
    }

    return (
        <div className="grid gap-2">
            {entries.map(([key, value]) => {
                const inputId = `rule-param-${rule.ruleType}-${key}`;

                if (typeof value === "boolean") {
                    return (
                        <div className="flex items-center gap-2" key={key}>
                            <input
                                id={inputId}
                                type="checkbox"
                                checked={value}
                                disabled={!editable}
                                onChange={(event) =>
                                    onChange({
                                        ...rule.params,
                                        [key]: event.currentTarget.checked,
                                    })
                                }
                            />
                            <Label htmlFor={inputId}>{key}</Label>
                        </div>
                    );
                }

                if (typeof value === "number") {
                    return (
                        <div className="grid gap-1" key={key}>
                            <Label htmlFor={inputId} className="text-xs">
                                {key}
                            </Label>
                            <Input
                                id={inputId}
                                type="number"
                                value={value}
                                disabled={!editable}
                                onChange={(event) => {
                                    const nextValue = Number(
                                        event.currentTarget.value,
                                    );
                                    if (Number.isNaN(nextValue)) {
                                        return;
                                    }

                                    onChange({
                                        ...rule.params,
                                        [key]: nextValue,
                                    });
                                }}
                            />
                        </div>
                    );
                }

                if (typeof value === "string") {
                    return (
                        <div className="grid gap-1" key={key}>
                            <Label htmlFor={inputId} className="text-xs">
                                {key}
                            </Label>
                            <Input
                                id={inputId}
                                value={value}
                                disabled={!editable}
                                onChange={(event) =>
                                    onChange({
                                        ...rule.params,
                                        [key]: event.currentTarget.value,
                                    })
                                }
                            />
                        </div>
                    );
                }

                if (Array.isArray(value)) {
                    const isNumericArray =
                        value.length > 0 && typeof value[0] === "number";
                    const placeholder = isNumericArray
                        ? "10\n20\n30"
                        : "value-1\nvalue-2\nvalue-3";

                    return (
                        <div className="grid gap-1" key={key}>
                            <Label htmlFor={inputId} className="text-xs">
                                {key}
                            </Label>
                            <p className="text-[11px] text-muted-foreground">
                                Enter one value per line.
                            </p>
                            <textarea
                                id={inputId}
                                rows={3}
                                value={value
                                    .map((item) => String(item))
                                    .join("\n")}
                                disabled={!editable}
                                className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                                placeholder={placeholder}
                                onChange={(event) =>
                                    onChange({
                                        ...rule.params,
                                        [key]: parseArrayValues(
                                            event.currentTarget.value,
                                            value,
                                        ),
                                    })
                                }
                            />
                        </div>
                    );
                }

                return (
                    <div className="grid gap-1" key={key}>
                        <Label htmlFor={inputId} className="text-xs">
                            {key}
                        </Label>
                        <Input
                            id={inputId}
                            value={JSON.stringify(value)}
                            disabled={!editable}
                            onChange={(event) => {
                                try {
                                    onChange({
                                        ...rule.params,
                                        [key]: JSON.parse(
                                            event.currentTarget.value,
                                        ),
                                    });
                                } catch {
                                    // Keep current value until valid JSON is entered.
                                }
                            }}
                        />
                    </div>
                );
            })}
        </div>
    );
};
