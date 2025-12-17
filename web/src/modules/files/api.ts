import { fileSchema } from "@/modules/files/schema";
import { useMutation } from "@tanstack/react-query";

export const useUploadFileMutation = () =>
    useMutation({
        mutationFn: async (file: File) => {
            const formData = new FormData();
            formData.append("file", file);

            const response = await fetch("/api/files/upload", {
                method: "POST",
                body: formData,
            });

            return await fileSchema.parseAsync(await response.json());
        },
    });
