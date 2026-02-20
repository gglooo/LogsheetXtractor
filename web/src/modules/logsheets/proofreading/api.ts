import { fileQueryFn } from "@/modules/files/api";
import {
    extractedValueSchema,
    logsheetSchema,
} from "@/modules/logsheets/schema";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

export const useExtractedValueImage = (extractedValueId: string) =>
    useQuery({
        queryKey: ["extracted-value", extractedValueId, "image"],
        refetchOnWindowFocus: false,
        queryFn: async () =>
            fileQueryFn(`/api/extracted-values/${extractedValueId}/image`),
    });

export const useVerifyExtractedValueMutation = (logsheetId: string) => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async ({
            extractedValueId,
            correctedValue,
        }: {
            extractedValueId: string;
            correctedValue?: string;
        }) => {
            const response = await fetch(
                `/api/extracted-values/${extractedValueId}/verify`,
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({ correctedValue }),
                },
            );

            if (!response.ok) {
                throw new Error("Failed to verify extracted value");
            }

            return response.json();
        },
        onSuccess: () => {
            queryClient.invalidateQueries({
                queryKey: ["logsheets", logsheetId],
            });
        },
    });
};

export const useVerifyExtractedValuesMutation = (logsheetId: string) => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async ({
            extractedValueIds,
            correctedValue,
        }: {
            extractedValueIds: string[];
            correctedValue?: string;
        }) => {
            const response = await fetch(`/api/extracted-values/batch/verify`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    ids: extractedValueIds,
                    correctedValue,
                }),
            });

            if (!response.ok) {
                throw new Error("Failed to verify extracted values");
            }

            return response.json();
        },
        onSuccess: () => {
            queryClient.invalidateQueries({
                queryKey: ["logsheets", logsheetId],
            });
        },
    });
};

export const useCompleteProofreadingMutation = (logsheetId: string) => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async () => {
            const response = await fetch(
                `/api/logsheets/${logsheetId}/proofreading/complete`,
                {
                    method: "POST",
                },
            );

            if (!response.ok) {
                throw new Error("Failed to complete proofreading");
            }

            return await logsheetSchema.parseAsync(await response.json());
        },
        onSuccess: () => {
            queryClient.invalidateQueries({
                queryKey: ["logsheet", logsheetId],
            });
        },
    });
};

export const useResetProofreadingMutation = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (logsheetId: string) => {
            const response = await fetch(
                `/api/logsheets/${logsheetId}/proofreading/reset`,
                {
                    method: "POST",
                },
            );

            if (!response.ok) {
                throw new Error("Failed to reset proofreading");
            }

            return;
        },
        onSuccess: (_data, logsheetId) => {
            queryClient.invalidateQueries({
                queryKey: ["logsheets", logsheetId],
            });
        },
    });
};

export const useRandomUnverifiedExtractedValue = (enabled?: boolean) =>
    useQuery({
        queryKey: ["extracted-values", "unverified", "random"],
        refetchOnWindowFocus: false,
        enabled,
        queryFn: async () => {
            const response = await fetch(
                "/api/extracted-values/unverified/random",
            );

            if (response.status === 204) {
                return null;
            }

            if (!response.ok) {
                throw new Error("Failed to fetch random unverified value");
            }

            return extractedValueSchema.parseAsync(await response.json());
        },
    });

export const useNextLogsheetUnverifiedExtractedValues = (enabled: boolean) =>
    useQuery({
        queryKey: ["extracted-values", "unverified", "next-logsheet"],
        refetchOnWindowFocus: false,
        enabled,
        queryFn: async () => {
            const response = await fetch(
                "/api/extracted-values/unverified/next-logsheet",
            );

            if (response.status === 204) {
                return null;
            }

            if (!response.ok) {
                throw new Error(
                    "Failed to fetch next logsheet unverified value",
                );
            }

            return extractedValueSchema.parseAsync(await response.json());
        },
    });
