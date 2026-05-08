import {
    useCredentialsStatus,
    useDeleteCredentialsMutation,
    useSetCredentialsMutation,
} from "@/modules/settings/api";
import { renderHook, waitFor } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";
import {
    createQueryClientWrapper,
    createTestQueryClient,
} from "../../../utils/query-client";

const originalFetch = window.fetch;

afterEach(() => {
    window.fetch = originalFetch;
    vi.restoreAllMocks();
});

describe("settings api hooks", () => {
    it("returns unavailable status when endpoint responds with 204", async () => {
        const fetchMock = vi
            .fn()
            .mockResolvedValue(new Response(null, { status: 204 }));
        window.fetch = fetchMock as typeof fetch;

        const queryClient = createTestQueryClient();
        const { result } = renderHook(() => useCredentialsStatus(), {
            wrapper: createQueryClientWrapper(queryClient),
        });

        await waitFor(() => {
            expect(result.current.isSuccess).toBe(true);
        });

        expect(result.current.data).toEqual({
            available: false,
            hasUserCredentials: false,
        });
        expect(fetchMock).toHaveBeenCalledWith("/api/credentials/status");
    });

    it("parses credentials status payload", async () => {
        const fetchMock = vi.fn().mockResolvedValue(
            new Response(
                JSON.stringify({
                    available: true,
                    hasUserCredentials: true,
                }),
                {
                    status: 200,
                    headers: { "Content-Type": "application/json" },
                },
            ),
        );
        window.fetch = fetchMock as typeof fetch;

        const queryClient = createTestQueryClient();
        const { result } = renderHook(() => useCredentialsStatus(), {
            wrapper: createQueryClientWrapper(queryClient),
        });

        await waitFor(() => {
            expect(result.current.isSuccess).toBe(true);
        });

        expect(result.current.data).toEqual({
            available: true,
            hasUserCredentials: true,
        });
    });

    it("posts credentials and invalidates credentialsStatus query", async () => {
        const fetchMock = vi
            .fn()
            .mockResolvedValue(new Response(null, { status: 200 }));
        window.fetch = fetchMock as typeof fetch;

        const queryClient = createTestQueryClient();
        const invalidateSpy = vi
            .spyOn(queryClient, "invalidateQueries")
            .mockResolvedValue(undefined);

        const { result } = renderHook(() => useSetCredentialsMutation(), {
            wrapper: createQueryClientWrapper(queryClient),
        });

        await result.current.mutateAsync({
            keys: {
                Google: "g-key",
                Azure: "a-key",
                Amazon: "aws-key",
            },
        });

        expect(fetchMock).toHaveBeenCalledWith("/api/credentials", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({
                keys: {
                    Google: "g-key",
                    Azure: "a-key",
                    Amazon: "aws-key",
                },
            }),
        });

        await waitFor(() => {
            expect(invalidateSpy).toHaveBeenCalledWith({
                queryKey: ["credentialsStatus"],
            });
        });
    });

    it("throws when deleting credentials fails", async () => {
        const fetchMock = vi
            .fn()
            .mockResolvedValue(new Response(null, { status: 500 }));
        window.fetch = fetchMock as typeof fetch;

        const queryClient = createTestQueryClient();
        const { result } = renderHook(() => useDeleteCredentialsMutation(), {
            wrapper: createQueryClientWrapper(queryClient),
        });

        await expect(result.current.mutateAsync()).rejects.toThrow(
            "Failed to delete credentials",
        );
    });
});
