import { downloadFile, fileQueryFn } from "@/modules/files/api";
import { templateListSchema, templateSchema } from "@/modules/templates/schema";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

export const useTemplates = () =>
    useQuery({
        queryKey: ["templates"],
        queryFn: async () => {
            const response = await fetch("/api/templates");
            return await templateListSchema
                .array()
                .parseAsync(await response.json());
        },
    });

export const useTemplate = (templateId: string) =>
    useQuery({
        queryKey: ["template", templateId],
        queryFn: async () => {
            const response = await fetch(`/api/templates/${templateId}`);
            return await templateSchema.parseAsync(await response.json());
        },
    });

export const useCloneTemplateMutation = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationKey: ["cloneTemplate"],
        mutationFn: async ({
            templateId,
            newName,
            fileId,
        }: {
            templateId: string;
            newName: string;
            fileId: string;
        }) => {
            const response = await fetch(`/api/templates/${templateId}/clone`, {
                method: "POST",
                body: JSON.stringify({ newName, fileId }),
                headers: {
                    "Content-Type": "application/json",
                },
            });

            return await response.json();
        },
        onSuccess: async () => {
            await queryClient.invalidateQueries({ queryKey: ["templates"] });
        },
    });
};

export const useDeleteTemplateMutation = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationKey: ["deleteTemplate"],
        mutationFn: async (templateId: string) => {
            await fetch(`/api/templates/${templateId}`, {
                method: "DELETE",
            });

            return;
        },
        onSuccess: async () => {
            console.log("Invalidating templates query");
            await queryClient.invalidateQueries({ queryKey: ["templates"] });
        },
    });
};

export const useCreateTemplateMutation = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationKey: ["createTemplate"],
        mutationFn: async ({
            name,
            fileId,
            importedConfig,
        }: {
            name: string;
            fileId: string;
            importedConfig?: string;
        }) => {
            const response = await fetch(`/api/templates`, {
                method: "POST",
                body: JSON.stringify({ name, fileId, importedConfig }),
                headers: {
                    "Content-Type": "application/json",
                },
            });

            return await templateSchema.parseAsync(await response.json());
        },
        onSuccess: async () => {
            await queryClient.invalidateQueries({ queryKey: ["templates"] });
        },
    });
};

export const useExportConfigMutation = () => {
    return useMutation({
        mutationFn: async ({ templateId }: { templateId: string }) => {
            const { bytes, fileName, contentType } = await fileQueryFn(
                `api/templates/${templateId}/export-config`,
                "POST"
            );

            const blob = new Blob([bytes], { type: contentType || undefined });

            if (blob.size === 0) {
                throw new Error("Exported config is empty");
            }

            await downloadFile(blob, fileName);
        },
    });
};
