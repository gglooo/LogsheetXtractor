import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";
import { DropdownMenuItem } from "@/components/ui/dropdown-menu";
import { Form } from "@/components/ui/form";
import { CloneTemplateWizard } from "@/modules/templates/actions/clone-template-action/clone-template-wizard";
import { cloneTemplateSchema } from "@/modules/templates/schema";
import { Copy } from "lucide-react";
import { useState } from "react";
import { useIntl } from "react-intl";

export const CloneTemplateAction = ({ templateId }: { templateId: string }) => {
    const [showModal, setShowModal] = useState(false);
    const intl = useIntl();

    return (
        <>
            <DropdownMenuItem
                onSelect={(e) => {
                    e.preventDefault();
                    setShowModal(true);
                }}
            >
                <Copy className="mr-2 h-4 w-4" />
                {intl.formatMessage({
                    id: "templates.actions.clone",
                    defaultMessage: "Clone Template",
                })}
            </DropdownMenuItem>
            <Dialog open={showModal} onOpenChange={setShowModal}>
                <DialogContent className="sm:max-w-125">
                    <DialogHeader>
                        <DialogTitle>
                            {intl.formatMessage({
                                id: "templates.actions.clone.title",
                                defaultMessage: "Clone a template",
                            })}
                        </DialogTitle>
                        <DialogDescription>
                            {intl.formatMessage({
                                id: "templates.actions.clone.description",
                                defaultMessage:
                                    "Upload a new PDF to create a cloned template. All ROIs from the original template will be copied to the new one.",
                            })}
                        </DialogDescription>
                    </DialogHeader>
                    <Form
                        schema={cloneTemplateSchema}
                        defaultValues={{
                            name: "",
                            file: undefined,
                        }}
                    >
                        <CloneTemplateWizard
                            onClose={() => setShowModal(false)}
                            templateId={templateId}
                        />
                    </Form>
                </DialogContent>
            </Dialog>
        </>
    );
};
