import { z } from "zod";

export const credentialsStatusSchema = z.object({
    available: z.boolean(),
    hasUserCredentials: z.boolean(),
});

export type CredentialsStatusDto = z.infer<typeof credentialsStatusSchema>;

export const credentialTypeSchema = z.enum(["Google", "Azure", "Amazon"]);

export const setUserCredentialsSchema = z.object({
    keys: z.record(credentialTypeSchema, z.string().trim()),
});

export type SetUserCredentialsFormValues = z.infer<
    typeof setUserCredentialsSchema
>;
