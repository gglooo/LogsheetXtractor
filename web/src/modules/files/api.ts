import { fileSchema, type DownloadedFileType } from "@/modules/files/schema";
import { useMutation, useQuery } from "@tanstack/react-query";

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

export const useFile = (fileId: string) =>
    useQuery({
        queryKey: ["file", fileId],
        refetchOnWindowFocus: false,
        queryFn: async (): Promise<DownloadedFileType> => {
            const response = await fetch(`/api/files/${fileId}`);

            const blob = await response.blob();

            const contentType = response.headers.get("Content-Type");

            const contentDisposition = response.headers.get(
                "Content-Disposition"
            );
            let fileName = "downloaded_file";

            if (
                contentDisposition &&
                contentDisposition.includes("filename=")
            ) {
                fileName = contentDisposition
                    .split("filename=")[1]
                    .split(";")[0]
                    .replace(/"/g, "");
            }

            return {
                bytes: await blob.arrayBuffer(),
                fileName,
                contentType,
            };
        },
    });
