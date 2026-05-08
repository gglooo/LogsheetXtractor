import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import type { SetUserCredentialsFormValues } from "./schema";
import { credentialsStatusSchema } from "./schema";

export const useCredentialsStatus = () =>
    useQuery({
        queryKey: ["credentialsStatus"],
        queryFn: async () => {
            const response = await fetch("/api/credentials/status");
            if (response.status === 204) {
                return { available: false, hasUserCredentials: false };
            }
            return await credentialsStatusSchema.parseAsync(
                await response.json(),
            );
        },
    });

export const useSetCredentialsMutation = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationKey: ["setCredentials"],
        mutationFn: async (values: SetUserCredentialsFormValues) => {
            const response = await fetch("/api/credentials", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(values),
            });

            if (!response.ok) {
                throw new Error("Failed to set credentials");
            }
        },
        onSuccess: async () => {
            await queryClient.invalidateQueries({
                queryKey: ["credentialsStatus"],
            });
        },
    });
};

export const useDeleteCredentialsMutation = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationKey: ["deleteCredentials"],
        mutationFn: async () => {
            const response = await fetch("/api/credentials", {
                method: "DELETE",
            });

            if (!response.ok) {
                throw new Error("Failed to delete credentials");
            }
        },
        onSuccess: async () => {
            await queryClient.invalidateQueries({
                queryKey: ["credentialsStatus"],
            });
        },
    });
};
