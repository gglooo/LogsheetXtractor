import { downloadFile, fileQueryFn } from "@/modules/files/api";
import {
    logsheetListSchema,
    logsheetSchema,
    uploadLogsheetsRequestSchema,
} from "@/modules/logsheets/schema";
import type { Position } from "@/modules/pdf/hooks/use-draw-rectangle";
import { useUserSettings } from "@/modules/settings/hooks/useUserSettings";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

export const useLogsheets = (templateId: string) =>
    useQuery({
        queryKey: ["logsheets", templateId],
        queryFn: async () => {
            const response = await fetch(
                `/api/templates/${templateId}/logsheets`,
            );
            return await logsheetListSchema
                .array()
                .parseAsync(await response.json());
        },
    });

export const useLogsheet = (logsheetId: string) =>
    useQuery({
        queryKey: ["logsheets", logsheetId],
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
    const { userSettings } = useUserSettings();

    return useMutation({
        mutationKey: ["processLogsheet"],
        mutationFn: async (logsheetId: string) => {
            const response = await fetch(
                `/api/logsheets/${logsheetId}/process`,
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        options: {
                            uglyCheckboxes: userSettings.uglyCheckboxes,
                        },
                    }),
                },
            );

            if (!response.ok) {
                throw new Error(response.statusText);
            }
        },
        onSuccess: async () => {
            await queryClient.invalidateQueries({ queryKey: ["logsheets"] });
        },
    });
};

export const useProcessLogsheetsMutation = () => {
    const queryClient = useQueryClient();
    const { userSettings } = useUserSettings();

    return useMutation({
        mutationKey: ["processLogsheets"],
        mutationFn: async (logsheetIds: string[]) => {
            const response = await fetch(`/api/logsheets/batch/process`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    logsheetIds,
                    options: {
                        uglyCheckboxes: userSettings.uglyCheckboxes,
                    },
                }),
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

export const useUploadLogsheetsMutation = () => {
    const { userSettings } = useUserSettings();

    return useMutation({
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
            const payload = uploadLogsheetsRequestSchema.parse({
                templateId,
                backsideTemplateId,
                fileIds,
                performAutomaticAlignment:
                    userSettings.automaticAlignmentOnUpload,
            });

            const response = await fetch(`/api/logsheets/batch`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(payload),
            });

            if (!response.ok) {
                throw new Error(response.statusText);
            }

            return await logsheetSchema
                .array()
                .parseAsync(await response.json());
        },
    });
};

export const useAlignLogsheetMutation = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async ({
            logsheetId,
            frontside,
            backside,
        }: {
            logsheetId: string;
            frontside?: Position[];
            backside?: Position[];
        }) => {
            const response = await fetch(
                `/api/logsheets/${logsheetId}/alignment/set`,
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        alignment: {
                            frontside: frontside?.map((p) => ({
                                x: Math.round(p.x),
                                y: Math.round(p.y),
                            })),
                            backside: backside?.map((p) => ({
                                x: Math.round(p.x),
                                y: Math.round(p.y),
                            })),
                        },
                    }),
                },
            );

            if (!response.ok) {
                throw new Error(response.statusText);
            }
        },
        onSuccess: async () => {
            await queryClient.invalidateQueries({ queryKey: ["logsheets"] });
        },
    });
};

export const useAutomaticAlignLogsheetMutation = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (logsheetId: string) => {
            const response = await fetch(`/api/logsheets/${logsheetId}/align`, {
                method: "POST",
            });

            if (!response.ok) {
                throw new Error(response.statusText);
            }

            return logsheetSchema.parseAsync(await response.json());
        },
        onSuccess: async () => {
            await queryClient.invalidateQueries({ queryKey: ["logsheets"] });
        },
    });
};

export const useLogsheetImage = (
    logsheetId?: string | null,
    isBackSide?: boolean,
) =>
    useQuery({
        queryKey: ["logsheets", logsheetId, "image", isBackSide],
        refetchOnWindowFocus: false,
        queryFn: async () =>
            fileQueryFn(
                `/api/logsheets/${logsheetId}/image${isBackSide ? "?backside=true" : ""}`,
            ),
        retry: (_, error) =>
            !(error instanceof Error && error.message.includes("Not Found")),
        enabled: !!logsheetId,
    });

export const useExportLogsheetMutation = () =>
    useMutation({
        mutationFn: async ({ logsheetId }: { logsheetId: string }) => {
            const { bytes, fileName, contentType } = await fileQueryFn(
                `/api/logsheets/${logsheetId}/export`,
                { method: "POST" },
            );

            const blob = new Blob([bytes], { type: contentType ?? undefined });
            await downloadFile(blob, fileName);
        },
    });

export const useExportLogsheetsMutation = () =>
    useMutation({
        mutationFn: async (logsheetIds: string[]) => {
            const { bytes, fileName, contentType } = await fileQueryFn(
                `/api/logsheets/batch/export`,
                {
                    method: "POST",
                    body: JSON.stringify({ logsheetIds }),
                    headers: {
                        "Content-Type": "application/json",
                    },
                },
            );

            const blob = new Blob([bytes], { type: contentType ?? undefined });
            await downloadFile(blob, fileName);
        },
    });
