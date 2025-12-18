import { detectRoisResponseSchema } from "@/modules/rois/schema";
import { useMutation, useQueryClient } from "@tanstack/react-query";

export const useDetectRoisMutation = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (templateId: string) => {
            const response = await fetch(
                `/api/templates/${templateId}/detect-rois`,
                {
                    method: "POST",
                }
            );

            return await detectRoisResponseSchema.parseAsync(
                await response.json()
            );
        },
        onSuccess: () => {
            queryClient.invalidateQueries({
                queryKey: ["rois"],
            });
        },
    });
};
