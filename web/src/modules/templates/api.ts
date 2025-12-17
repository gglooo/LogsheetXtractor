import { templateListSchema } from "@/modules/templates/schema";
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
