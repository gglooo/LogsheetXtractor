import { logsheetListSchema, logsheetSchema } from "@/modules/logsheets/schema";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

export const useLogsheets = (templateId: string) =>
    useQuery({
        queryKey: ["logsheets", templateId],
        queryFn: async () => {
            const response = await fetch(
                `/api/templates/${templateId}/logsheets`
            );
            return await logsheetListSchema
                .array()
                .parseAsync(await response.json());
        },
    });

export const useLogsheet = (logsheetId: string) =>
    useQuery({
        queryKey: ["logsheet", logsheetId],
        queryFn: async () => {
            const response = await fetch(`/api/logsheets/${logsheetId}`);
            return await logsheetSchema.parseAsync(await response.json());
        },
    });

export const useDeleteLogsheetMutation = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationKey: ["deleteLogsheet"],
        mutationFn: async (logsheetId: string) => {
            await fetch(`/api/logsheets/${logsheetId}`, {
                method: "DELETE",
            });
        },
        onSuccess: async () => {
            await queryClient.invalidateQueries({ queryKey: ["logsheets"] });
        },
    });
};

export const useProcessLogsheetMutation = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationKey: ["processLogsheet"],
        mutationFn: async (logsheetId: string) => {
            const response = await fetch(
                `/api/logsheets/${logsheetId}/process`,
                {
                    method: "POST",
                }
            );

            if (!response.ok) {
                throw new Error(response.statusText);
            }

            return await response.json();
        },
        onSuccess: async () => {
            await queryClient.invalidateQueries({ queryKey: ["logsheets"] });
        },
    });
};

export const useProcessLogsheetsMutation = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationKey: ["processLogsheets"],
        mutationFn: async (logsheetIds: string[]) => {
            const response = await fetch(`/api/logsheets/batch/process`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ logsheetIds }),
            });

            if (!response.ok) {
                throw new Error(response.statusText);
            }

            return await response.json();
        },
        onSuccess: async () => {
            await queryClient.invalidateQueries({ queryKey: ["logsheets"] });
        },
    });
};

export const useDeleteLogsheetsMutation = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationKey: ["deleteLogsheets"],
        mutationFn: async (logsheetIds: string[]) => {
            const response = await fetch(`/api/logsheets/batch`, {
                method: "DELETE",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ logsheetIds }),
            });

            if (!response.ok) {
                throw new Error(response.statusText);
            }
        },
        onSuccess: async () => {
            await queryClient.invalidateQueries({ queryKey: ["logsheets"] });
        },
    });
};

export const useUploadLogsheetsMutation = () =>
    useMutation({
        mutationKey: ["uploadLogsheets"],
        mutationFn: async ({
            templateId,
            backsideTemplateId,
            fileIds,
        }: {
            templateId: string;
            backsideTemplateId?: string;
            fileIds: string[];
        }) => {
            const response = await fetch(`/api/logsheets/batch`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    templateId,
                    backsideTemplateId,
                    fileIds,
                }),
            });

            if (!response.ok) {
                throw new Error(response.statusText);
            }

            return await logsheetSchema
                .array()
                .parseAsync(await response.json());
        },
    });
