import { Button } from "@/components/ui/button";
import {
    Sheet,
    SheetContent,
    SheetDescription,
    SheetHeader,
    SheetTitle,
    SheetTrigger,
} from "@/components/ui/sheet";
import { roiValidationConditionGroupSchema } from "@/modules/rois/validation/schema";
import type { RoiType } from "@/modules/rois/schema";
import { RoiValidationConditionBuilder } from "@/modules/template-editor/sidebar/roi-validation/components/roi-validation-condition-builder";
import { SelectPreset } from "@/modules/template-editor/sidebar/roi-validation/components/select-preset";
import { useState } from "react";
import { useForm, useWatch } from "react-hook-form";
import { useIntl } from "react-intl";
import z from "zod";

type RoiValidationConditionSheetProps = {
    selectedRoi: RoiType;
    editable: boolean;
    onChangeValidationCondition: (
        validationCondition: RoiType["validationCondition"],
    ) => void;
};

const roiValidationDraftSchema = z.object({
    validationCondition: roiValidationConditionGroupSchema.nullable(),
});

type RoiValidationDraftFormValues = z.infer<typeof roiValidationDraftSchema>;

export const RoiValidationConditionSheet = ({
    selectedRoi,
    editable,
    onChangeValidationCondition,
}: RoiValidationConditionSheetProps) => {
    const intl = useIntl();
    const [open, setOpen] = useState(false);

    const form = useForm<RoiValidationDraftFormValues>({
        defaultValues: {
            validationCondition: selectedRoi.validationCondition,
        },
        mode: "onChange",
    });

    const draftValidationCondition = useWatch({
        control: form.control,
        name: "validationCondition",
    });
    const isDraftDirty = form.formState.isDirty;

    const handleApply = (values: RoiValidationDraftFormValues) => {
        const parsedValues = roiValidationDraftSchema.parse(values);
        onChangeValidationCondition(parsedValues.validationCondition);
        form.reset(parsedValues);
        setOpen(false);
    };

    const handleOpenChange = (nextOpen: boolean) => {
        setOpen(nextOpen);
        if (nextOpen) {
            form.reset({
                validationCondition: selectedRoi.validationCondition,
            });
        }
    };

    return (
        <div className="rounded-md border p-2 space-y-2">
            <div className="space-y-1">
                <div className="text-xs font-semibold">
                    {intl.formatMessage({
                        id: "template-editor.roi-validation.title",
                        defaultMessage: "Validation conditions",
                    })}
                </div>
                <div className="text-xs text-muted-foreground">
                    {selectedRoi.validationCondition
                        ? intl.formatMessage({
                              id: "template-editor.roi-validation.state.enabled",
                              defaultMessage: "Configured",
                          })
                        : intl.formatMessage({
                              id: "template-editor.roi-validation.state.disabled",
                              defaultMessage: "Not configured",
                          })}
                </div>
            </div>
            <Sheet open={open} onOpenChange={handleOpenChange}>
                <SheetTrigger asChild>
                    <button
                        type="button"
                        className="w-full h-8 rounded-md border px-3 text-sm font-medium text-left hover:bg-accent disabled:cursor-not-allowed disabled:opacity-50"
                    >
                        {intl.formatMessage({
                            id: editable
                                ? "template-editor.roi-validation.open-editor"
                                : "template-editor.roi-validation.open-viewer",
                            defaultMessage: editable ? "Edit" : "View",
                        })}
                    </button>
                </SheetTrigger>
                <SheetContent className="w-[92vw] sm:max-w-2xl bg-secondary flex flex-col">
                    <SheetHeader className="px-4 pt-4">
                        <SheetTitle>
                            {intl.formatMessage({
                                id: "template-editor.roi-validation.sheet.title",
                                defaultMessage: "Validation conditions",
                            })}
                        </SheetTitle>
                        <SheetDescription>
                            {intl.formatMessage(
                                {
                                    id: editable
                                        ? "template-editor.roi-validation.sheet.description"
                                        : "template-editor.roi-validation.sheet.description.readonly",
                                    defaultMessage: editable
                                        ? "Configure condition tree for ROI {variableName}."
                                        : "View condition tree for ROI {variableName}.",
                                },
                                { variableName: selectedRoi.variableName },
                            )}
                        </SheetDescription>
                    </SheetHeader>

                    <form
                        className="flex min-h-0 flex-1 flex-col"
                        onSubmit={form.handleSubmit(handleApply)}
                    >
                        <div className="flex-1 overflow-y-auto px-4 pb-4 flex flex-col gap-4">
                            <SelectPreset
                                roiType={selectedRoi.type}
                                editable={editable}
                                onSelect={(validationCondition) =>
                                    form.setValue(
                                        "validationCondition",
                                        validationCondition,
                                        {
                                            shouldDirty: true,
                                            shouldTouch: true,
                                        },
                                    )
                                }
                            />
                            <RoiValidationConditionBuilder
                                roiType={selectedRoi.type}
                                validationCondition={draftValidationCondition}
                                editable={editable}
                                onChange={(validationCondition) =>
                                    form.setValue(
                                        "validationCondition",
                                        validationCondition,
                                        {
                                            shouldDirty: true,
                                            shouldTouch: true,
                                        },
                                    )
                                }
                            />
                        </div>
                        {editable && isDraftDirty ? (
                            <div className="border-t px-4 py-3 bg-secondary">
                                <div className="flex justify-end">
                                    <Button
                                        size="lg"
                                        type="submit"
                                        disabled={!editable}
                                    >
                                        {intl.formatMessage({
                                            id: "template-editor.roi-validation.apply",
                                            defaultMessage: "Apply",
                                        })}
                                    </Button>
                                </div>
                            </div>
                        ) : null}
                    </form>
                </SheetContent>
            </Sheet>
        </div>
    );
};
