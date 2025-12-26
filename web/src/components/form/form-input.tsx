import {
    FormControl,
    FormField,
    FormItem,
    FormLabel,
    FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { forwardRef } from "react";

type Props = {
    name: string;
    label: string;
} & React.ComponentProps<"input">;

export const FormInput = forwardRef<HTMLInputElement, Props>(
    ({ name, label, ...props }, ref) => {
        return (
            <FormField
                name={name}
                render={({ field }) => (
                    <FormItem>
                        <FormLabel>{label}</FormLabel>
                        <FormControl>
                            <Input {...props} {...field} ref={ref} />
                        </FormControl>
                        <FormMessage />
                    </FormItem>
                )}
            />
        );
    }
);
