import { fileSchema } from "@/modules/files/schema";
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

const fileQueryFn = async (fileId: string) => {
    const response = await fetch(`/api/files/${fileId}`);

    const blob = await response.blob();

    const contentType = response.headers.get("Content-Type");

    const contentDisposition = response.headers.get("Content-Disposition");
    let fileName = "downloaded_file";

    if (contentDisposition && contentDisposition.includes("filename=")) {
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
};

export const useFile = (fileId: string) =>
    useQuery({
        queryKey: ["file", fileId],
        refetchOnWindowFocus: false,
        queryFn: () => fileQueryFn(fileId),
    });

export const useFileDownloadMutation = (fileId: string) =>
    useMutation({
        mutationKey: ["downloadFile", fileId],
        mutationFn: async () => {
            const { bytes, fileName, contentType } = await fileQueryFn(fileId);

            const blob = new Blob([bytes], { type: contentType || undefined });
            const url = window.URL.createObjectURL(blob);
            const link = document.createElement("a");
            link.href = url;
            link.setAttribute("download", fileName);
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            window.URL.revokeObjectURL(url);
        },
    });
