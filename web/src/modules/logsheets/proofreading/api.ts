import { fileQueryFn } from "@/modules/files/api";
import { logsheetSchema } from "@/modules/logsheets/schema";
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
                }
            );

            if (!response.ok) {
                throw new Error("Failed to verify extracted value");
            }

            return response.json();
        },
        onSuccess: () => {
            queryClient.invalidateQueries({
                queryKey: ["logsheet", logsheetId],
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
                }
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
