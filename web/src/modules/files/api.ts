import { fileSchema } from "@/modules/files/schema";
import { useMutation, useQuery } from "@tanstack/react-query";

const IMMUTABLE_FILE_STALE_TIME = Infinity;

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

export const fileQueryFn = async (url: string, init?: RequestInit) => {
    const response = await fetch(url, init);

    if (!response.ok) {
        throw new Error(response.statusText);
    }

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

export const useFile = (fileId: string | undefined | null) =>
    useQuery({
        queryKey: ["file", fileId],
        refetchOnWindowFocus: false,
        staleTime: IMMUTABLE_FILE_STALE_TIME,
        queryFn: () => fileQueryFn(`/api/files/${fileId}`),
        enabled: !!fileId,
    });

export const usePdfFileImage = (fileId: string | undefined | null) =>
    useQuery({
        queryKey: ["file-image", fileId],
        refetchOnWindowFocus: false,
        staleTime: IMMUTABLE_FILE_STALE_TIME,
        queryFn: () => fileQueryFn(`/api/files/${fileId}/image`),
        retry: (_, error) =>
            !(error instanceof Error && error.message.includes("Not Found")),
        enabled: !!fileId,
    });

export const downloadFile = async (blob: Blob, filename: string) => {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;
    link.setAttribute("download", filename);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
};

export const useFileDownloadMutation = () => {
    return useMutation({
        mutationFn: async ({ fileId }: { fileId: string }) => {
            const { bytes, fileName, contentType } = await fileQueryFn(
                `/api/files/${fileId}`,
            );

            const blob = new Blob([bytes], { type: contentType || undefined });
            await downloadFile(blob, fileName);
        },
    });
};
