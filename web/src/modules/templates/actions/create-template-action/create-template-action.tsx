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
import { useState } from "react";
import { useIntl } from "react-intl";

export const CreateTemplateAction = () => {
    const intl = useIntl();
    const [showModal, setShowModal] = useState(false);

    return (
        <>
            <Button onClick={() => setShowModal(true)}>
                {intl.formatMessage({
                    id: "templates.actions.createTemplate",
                    defaultMessage: "Create template",
                })}
            </Button>

            <Dialog open={showModal} onOpenChange={setShowModal}>
                <DialogContent>
                    <DialogHeader>
                        <DialogTitle>
                            {intl.formatMessage({
                                id: "templates.actions.createTemplate",
                                defaultMessage: "Create template",
                            })}
                        </DialogTitle>
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
