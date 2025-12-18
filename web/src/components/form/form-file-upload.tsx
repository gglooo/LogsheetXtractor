import { PdfFileUpload } from "@/components/pdf-file-upload";
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
};

export const FormFileUpload = ({ name, label, ...props }: Props) => {
    return (
        <FormField
            name={name}
            render={({ field }) => (
                <FormItem>
                    <FormLabel>{label}</FormLabel>
                    <FormControl>
                        <PdfFileUpload
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
