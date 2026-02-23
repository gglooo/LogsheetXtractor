import { Button } from "@/components/ui/button";
import { Form } from "@/components/ui/form";
import { Spinner } from "@/components/ui/spinner";
import {
    useCredentialsStatus,
    useSetCredentialsMutation,
} from "@/modules/settings/api";
import { CredentialsFormItem } from "@/modules/settings/components/credentials-form-item";
import {
    credentialTypeSchema,
    setUserCredentialsSchema,
    type SetUserCredentialsFormValues,
} from "@/modules/settings/schema";
import { zodResolver } from "@hookform/resolvers/zod";
import { Key } from "lucide-react";
import { useForm } from "react-hook-form";
import { useIntl } from "react-intl";
import { toast } from "sonner";

export const CredentialsAction = () => {
    const intl = useIntl();
    const setCredentialsMutation = useSetCredentialsMutation();
    const credentialsStatus = useCredentialsStatus();

    const form = useForm<SetUserCredentialsFormValues>({
        resolver: zodResolver(setUserCredentialsSchema),
        defaultValues: {
            keys: {
                Google: "",
                Azure: "",
                Amazon: "",
            },
        },
    });

    const onSubmit = async (values: SetUserCredentialsFormValues) => {
        try {
            await setCredentialsMutation.mutateAsync(values);
            toast.success(
                intl.formatMessage({
                    id: "settings.credentials.success",
                    defaultMessage: "Credentials successfully saved.",
                }),
            );
            form.reset({
                keys: {
                    Google: "",
                    Azure: "",
                    Amazon: "",
                },
            });
        } catch (error) {
            toast.error(
                intl.formatMessage({
                    id: "settings.credentials.error",
                    defaultMessage: "Failed to save credentials.",
                }),
            );
            console.error("Error setting credentials:", error);
        }
    };

    if (credentialsStatus.isLoading) {
        return <Spinner className="mt-4" />;
    }

    return (
        <div className="flex flex-col gap-6 max-w-xl">
            <div>
                <h3 className="text-lg font-medium flex items-center gap-2">
                    <Key className="h-5 w-5" />
                    {intl.formatMessage({
                        id: "settings.credentials.title",
                        defaultMessage: "OCR API Credentials",
                    })}
                </h3>
                <p className="text-sm text-muted-foreground">
                    {intl.formatMessage({
                        id: "settings.credentials.description",
                        defaultMessage:
                            "Enter your personal API keys below to override the system defaults.",
                    })}
                </p>
            </div>

            <Form schema={setUserCredentialsSchema} {...form}>
                <div className="space-y-4">
                    {credentialTypeSchema.options.map((provider) => (
                        <CredentialsFormItem
                            key={provider}
                            provider={provider}
                            form={form}
                        />
                    ))}

                    <Button
                        type="button"
                        onClick={form.handleSubmit(onSubmit)}
                        disabled={setCredentialsMutation.isPending}
                    >
                        {setCredentialsMutation.isPending ? (
                            <Spinner className="mr-2" />
                        ) : null}
                        {intl.formatMessage({
                            id: "settings.credentials.submit",
                            defaultMessage: "Save credentials",
                        })}
                    </Button>
                </div>
            </Form>
        </div>
    );
};
