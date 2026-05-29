import {
    FormControl,
    FormField,
    FormItem,
    FormLabel,
    FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";
import { forwardRef } from "react";

type Props = {
    name: string;
    label: string;
    labelClassname?: string;
} & React.ComponentProps<"input">;

export const FormInput = forwardRef<HTMLInputElement, Props>(
    ({ name, label, labelClassname, readOnly, className, ...props }, ref) => {
        return (
            <FormField
                name={name}
                render={({ field }) => (
                    <FormItem>
                        <FormLabel className={labelClassname}>
                            {label}
                        </FormLabel>
                        <FormControl>
                            <Input
                                className={cn(
                                    className,
                                    readOnly && "cursor-not-allowed opacity-70",
                                )}
                                {...props}
                                {...field}
                                ref={ref}
                                readOnly={readOnly}
                            />
                        </FormControl>
                        <FormMessage />
                    </FormItem>
                )}
            />
        );
    },
);
