import {
    useCompleteProofreadingMutation,
    useExtractedValueImage,
    useRandomUnverifiedExtractedValue,
    useVerifyExtractedValueMutation,
} from "@/modules/logsheets/proofreading/api";
import { renderHook, waitFor } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";
import {
    createQueryClientWrapper,
    createTestQueryClient,
} from "../../../../utils/query-client";
import { fileQueryFn } from "@/modules/files/api";

vi.mock("@/modules/files/api", () => ({
    fileQueryFn: vi.fn(),
}));

const originalFetch = window.fetch;

const now = new Date().toISOString();
const ids = {
    template: "11111111-1111-4111-8111-111111111111",
    file: "22222222-2222-4222-8222-222222222222",
    logsheet: "33333333-3333-4333-8333-333333333333",
    roi: "44444444-4444-4444-8444-444444444444",
    extracted: "55555555-5555-4555-8555-555555555555",
};

afterEach(() => {
    window.fetch = originalFetch;
    vi.restoreAllMocks();
});

describe("logsheets proofreading api hooks", () => {
    it("fetches extracted value image through fileQueryFn", async () => {
        vi.mocked(fileQueryFn).mockResolvedValue({
            bytes: new ArrayBuffer(4),
            fileName: "value.png",
            contentType: "image/png",
        });

        const queryClient = createTestQueryClient();
        const { result } = renderHook(() => useExtractedValueImage(ids.extracted), {
            wrapper: createQueryClientWrapper(queryClient),
        });

        await waitFor(() => {
            expect(result.current.isSuccess).toBe(true);
        });

        expect(fileQueryFn).toHaveBeenCalledWith(
            `/api/extracted-values/${ids.extracted}/image`,
        );
    });

    it("verifies extracted value and invalidates related logsheet query", async () => {
        const fetchMock = vi.fn().mockResolvedValue(
            new Response(JSON.stringify({ ok: true }), {
                status: 200,
                headers: { "Content-Type": "application/json" },
            }),
        );
        window.fetch = fetchMock as typeof fetch;

        const queryClient = createTestQueryClient();
        const invalidateSpy = vi
            .spyOn(queryClient, "invalidateQueries")
            .mockResolvedValue(undefined);

        const { result } = renderHook(
            () => useVerifyExtractedValueMutation(ids.logsheet),
            {
                wrapper: createQueryClientWrapper(queryClient),
            },
        );

        await result.current.mutateAsync({
            extractedValueId: ids.extracted,
            correctedValue: "corrected",
        });

        const [url, init] = fetchMock.mock.calls[0];
        expect(url).toBe(`/api/extracted-values/${ids.extracted}/verify`);
        expect(init?.method).toBe("POST");
        expect(init?.body).toBe(JSON.stringify({ correctedValue: "corrected" }));

        await waitFor(() => {
            expect(invalidateSpy).toHaveBeenCalledWith({
                queryKey: ["logsheets", ids.logsheet],
            });
        });
    });

    it("returns null for random unverified value when backend returns 204", async () => {
        const fetchMock = vi
            .fn()
            .mockResolvedValue(new Response(null, { status: 204 }));
        window.fetch = fetchMock as typeof fetch;

        const queryClient = createTestQueryClient();
        const { result } = renderHook(
            () => useRandomUnverifiedExtractedValue(true),
            {
                wrapper: createQueryClientWrapper(queryClient),
            },
        );

        await waitFor(() => {
            expect(result.current.isSuccess).toBe(true);
        });

        expect(result.current.data).toBeNull();
    });

    it("completes proofreading and invalidates logsheet detail cache", async () => {
        const fetchMock = vi.fn().mockResolvedValue(
            new Response(
                JSON.stringify({
                    id: ids.logsheet,
                    createdAt: now,
                    updatedAt: null,
                    deletedAt: null,
                    template: {
                        id: ids.template,
                        createdAt: now,
                        updatedAt: null,
                        deletedAt: null,
                        name: "Template",
                        parentId: null,
                        backsideTemplateId: null,
                        fileId: ids.file,
                        roiCount: 0,
                        logsheetCount: 0,
                        width: 100,
                        height: 200,
                    },
                    file: {
                        id: ids.file,
                        createdAt: now,
                        updatedAt: null,
                        deletedAt: null,
                        fileName: "logsheet.pdf",
                        contentType: "application/pdf",
                        sizeBytes: 100,
                    },
                    status: "Completed",
                    processedAt: now,
                    alignmentData: null,
                    extractedValues: [],
                }),
                {
                    status: 200,
                    headers: { "Content-Type": "application/json" },
                },
            ),
        );
        window.fetch = fetchMock as typeof fetch;

        const queryClient = createTestQueryClient();
        const invalidateSpy = vi
            .spyOn(queryClient, "invalidateQueries")
            .mockResolvedValue(undefined);

        const { result } = renderHook(
            () => useCompleteProofreadingMutation(ids.logsheet),
            {
                wrapper: createQueryClientWrapper(queryClient),
            },
        );

        await result.current.mutateAsync();

        await waitFor(() => {
            expect(invalidateSpy).toHaveBeenCalledWith({
                queryKey: ["logsheets", ids.logsheet],
            });
        });
    });
});
