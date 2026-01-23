import { FileUpload, type FileUploadProps } from "@/components/file-upload";
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
    onChange?: (file: File | File[] | null) => void;
} & Omit<FileUploadProps, "file" | "onFileChange">;

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
                            onFileChange={(file) => {
                                field.onChange(file);
                                props.onChange?.(file);
                            }}
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
