import { FileUpload } from "@/components/file-upload";
import {
    FormControl,
    FormField,
    FormItem,
    FormLabel,
    FormMessage,
} from "@/components/ui/form";

type Props = {
    name: string;
    label: string;
    multiple?: boolean;
    accept?: string;
    validator?: (file: File) => boolean;
    placeholder?: string;
    dragDropText?: string;
};

export const FormFileUpload = ({ name, label, ...props }: Props) => {
    return (
        <FormField
            name={name}
            render={({ field }) => (
                <FormItem>
                    <FormLabel>{label}</FormLabel>
                    <FormControl>
                        <FileUpload
                            file={field.value}
                            onFileChange={field.onChange}
                            {...props}
                            {...field}
                        />
                    </FormControl>
                    <FormMessage />
                </FormItem>
            )}
        />
    );
};
