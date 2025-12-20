import {
    detectRoisResponseSchema,
    roiSchema,
    type DetectedRoiType,
} from "@/modules/rois/schema";
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

export const useSetRoisMutation = (templateId?: string) => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async ({ rois }: { rois: DetectedRoiType[] }) => {
            const response = await fetch(
                `/api/templates/${templateId}/rois/set`,
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({ rois }),
                }
            );

            return await roiSchema.array().parseAsync(await response.json());
        },
        onSuccess: () => {
            queryClient.invalidateQueries({
                queryKey: ["rois"],
            });
            queryClient.invalidateQueries({
                queryKey: ["template", templateId],
            });
        },
    });
};
