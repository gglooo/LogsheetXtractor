import {
    FormControl,
    FormField,
    FormItem,
    FormLabel,
    FormMessage,
} from "@/components/ui/form";
import { cn } from "@/lib/utils";

export type SelectOption = { label: string; value: string | number };

type Props = {
    name: string;
    label: string;
    options: SelectOption[];
} & React.ComponentProps<"select">;

export const FormSelect = ({
    name,
    label,
    options,
    className,
    ...props
}: Props) => {
    return (
        <FormField
            name={name}
            render={({ field }) => (
                <FormItem>
                    <FormLabel>{label}</FormLabel>
                    <FormControl>
                        <select
                            className={cn(
                                "flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-base shadow-sm transition-colors file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50 md:text-sm",
                                className
                            )}
                            {...props}
                            {...field}
                        >
                            {options.map((option) => (
                                <option key={option.value} value={option.value}>
                                    {option.label}
                                </option>
                            ))}
                        </select>
                    </FormControl>
                    <FormMessage />
                </FormItem>
            )}
        />
    );
};
