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
import { useIntl } from "react-intl";

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
                        {intl.formatMessage({
                            id: `settings.credentials.${provider.toLowerCase()}.label`,
                            defaultMessage: `${provider} API key`,
                        })}
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
