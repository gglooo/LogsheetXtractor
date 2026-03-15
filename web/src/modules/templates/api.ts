import {
    downloadFile,
    fileQueryFn,
    useUploadFileMutation,
} from "@/modules/files/api";
import type { CreateTemplateFormValues } from "@/modules/templates/schema";
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
            backside,
        }: {
            templateId: string;
            newName: string;
            fileId: string;
            backside?: { name: string; fileId: string };
        }) => {
            const response = await fetch(`/api/templates/${templateId}/clone`, {
                method: "POST",
                body: JSON.stringify({ newName, fileId, backside }),
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

export const useAddTemplateBacksideMutation = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationKey: ["addTemplateBackside"],
        mutationFn: async ({
            templateId,
            name,
            fileId,
        }: {
            templateId: string;
            name: string;
            fileId: string;
        }) => {
            const response = await fetch(`/api/templates/${templateId}/backside`, {
                method: "POST",
                body: JSON.stringify({ name, fileId }),
                headers: {
                    "Content-Type": "application/json",
                },
            });

            return await templateSchema.parseAsync(await response.json());
        },
        onSuccess: async (_, variables) => {
            await queryClient.invalidateQueries({ queryKey: ["templates"] });
            await queryClient.invalidateQueries({
                queryKey: ["template", variables.templateId],
            });
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
            await queryClient.invalidateQueries({ queryKey: ["templates"] });
        },
    });
};

export const useCreateTemplateMutation = () => {
    const queryClient = useQueryClient();
    const uploadFileMutation = useUploadFileMutation();

    return useMutation({
        mutationKey: ["createTemplate"],
        mutationFn: async (values: CreateTemplateFormValues) => {
            let importedConfig: string | undefined;
            if (values.importedConfig) {
                importedConfig = await values.importedConfig.text();
            }

            const uploadedFile = await uploadFileMutation.mutateAsync(
                values.file,
            );

            let backside;
            if (values.backside) {
                let backsideImportedConfig: string | undefined;
                if (values.backside.importedConfig) {
                    backsideImportedConfig =
                        await values.backside.importedConfig.text();
                }

                const backsideUploadedFile =
                    await uploadFileMutation.mutateAsync(values.backside.file);

                backside = {
                    name: values.backside.name,
                    fileId: backsideUploadedFile.id,
                    importedConfig: backsideImportedConfig,
                };
            }

            const response = await fetch(`/api/templates`, {
                method: "POST",
                body: JSON.stringify({
                    name: values.name,
                    fileId: uploadedFile.id,
                    importedConfig,
                    backside,
                }),
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
                { method: "POST" },
            );

            const blob = new Blob([bytes], { type: contentType || undefined });

            if (blob.size === 0) {
                throw new Error("Exported config is empty");
            }

            await downloadFile(blob, fileName);
        },
    });
};
