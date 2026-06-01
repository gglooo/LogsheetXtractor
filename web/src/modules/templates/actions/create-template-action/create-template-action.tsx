import { Button } from "@/components/ui/button";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";
import { Form } from "@/components/ui/form";
import { CreateTemplateWizard } from "@/modules/templates/actions/create-template-action/create-template-wizard";
import { createTemplateSchema } from "@/modules/templates/schema";
import type { ReactNode } from "react";
import { useState } from "react";
import { useIntl } from "react-intl";

type CreateTemplateActionProps = {
    trigger?: (open: () => void, label: string) => ReactNode;
};

export const CreateTemplateAction = ({ trigger }: CreateTemplateActionProps) => {
    const intl = useIntl();
    const [showModal, setShowModal] = useState(false);
    const label = intl.formatMessage({
        id: "templates.actions.createTemplate",
        defaultMessage: "Create template",
    });
    const openDialog = () => setShowModal(true);

    return (
        <>
            {trigger ? (
                trigger(openDialog, label)
            ) : (
                <Button onClick={openDialog}>{label}</Button>
            )}

            <Dialog open={showModal} onOpenChange={setShowModal}>
                <DialogContent>
                    <DialogHeader>
                        <DialogTitle>{label}</DialogTitle>
                        <DialogDescription>
                            {intl.formatMessage({
                                id: "templates.actions.createTemplate.description",
                                defaultMessage:
                                    "Create a new template by uploading a PDF file.",
                            })}
                        </DialogDescription>
                    </DialogHeader>

                    <Form
                        schema={createTemplateSchema}
                        defaultValues={{
                            name: "",
                            file: undefined,
                            importedConfig: undefined,
                        }}
                    >
                        <CreateTemplateWizard
                            onClose={() => {
                                setShowModal(false);
                            }}
                        />
                    </Form>
                </DialogContent>
            </Dialog>
        </>
    );
};
