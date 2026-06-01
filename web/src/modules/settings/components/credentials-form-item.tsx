import {
    FormControl,
    FormField,
    FormItem,
    FormLabel,
    FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { type SetUserCredentialsFormValues } from "@/modules/settings/schema";
import { type UseFormReturn } from "react-hook-form";
import { defineMessages, useIntl } from "react-intl";

const messages = defineMessages({
    Google: {
        id: "settings.credentials.google.label",
        defaultMessage: "Google API key",
    },
    Azure: {
        id: "settings.credentials.azure.label",
        defaultMessage: "Azure API key",
    },
    Amazon: {
        id: "settings.credentials.amazon.label",
        defaultMessage: "Amazon API key",
    },
});

interface CredentialsFormItemProps {
    provider: string;
    form: UseFormReturn<SetUserCredentialsFormValues>;
}

export const CredentialsFormItem = ({
    provider,
    form,
}: CredentialsFormItemProps) => {
    const intl = useIntl();

    return (
        <FormField
            control={form.control}
            name={`keys.${provider as keyof SetUserCredentialsFormValues["keys"]}`}
            render={({ field }) => (
                <FormItem>
                    <FormLabel>
                        {intl.formatMessage(
                            messages[provider as keyof typeof messages],
                        )}
                    </FormLabel>
                    <FormControl>
                        <Input
                            type="password"
                            placeholder={"{...}"}
                            {...field}
                        />
                    </FormControl>
                    <FormMessage />
                </FormItem>
            )}
        />
    );
};
