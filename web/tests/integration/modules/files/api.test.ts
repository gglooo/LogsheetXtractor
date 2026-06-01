import { fileQueryFn, useFile, usePdfFileImage } from "@/modules/files/api";
import { renderHook, waitFor } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";
import {
    createQueryClientWrapper,
    createTestQueryClient,
} from "../../../utils/query-client";

const originalFetch = window.fetch;
const fileId = "11111111-1111-4111-8111-111111111111";

afterEach(() => {
    window.fetch = originalFetch;
    vi.restoreAllMocks();
});

describe("files api", () => {
    it("fileQueryFn returns bytes and filename from content-disposition", async () => {
        const fetchMock = vi.fn().mockResolvedValue(
            new Response("pdf", {
                status: 200,
                headers: {
                    "Content-Type": "application/pdf",
                    "Content-Disposition": 'attachment; filename="template.pdf"',
                },
            }),
        );
        window.fetch = fetchMock as typeof fetch;

        const result = await fileQueryFn(`/api/files/${fileId}`);

        expect(result.fileName).toBe("template.pdf");
        expect(result.contentType).toBe("application/pdf");
        expect(result.bytes.byteLength).toBe(3);
    });

    it("fileQueryFn rejects non-ok file responses", async () => {
        window.fetch = vi
            .fn()
            .mockResolvedValue(new Response(null, { status: 404, statusText: "Not Found" })) as typeof fetch;

        await expect(fileQueryFn(`/api/files/${fileId}`)).rejects.toThrow(
            "Not Found",
        );
    });

    it("useFile does not fetch without an id", () => {
        const fetchMock = vi.fn();
        window.fetch = fetchMock as typeof fetch;

        renderHook(() => useFile(undefined), {
            wrapper: createQueryClientWrapper(createTestQueryClient()),
        });

        expect(fetchMock).not.toHaveBeenCalled();
    });

    it("usePdfFileImage fetches immutable image endpoint when id is present", async () => {
        const fetchMock = vi.fn().mockResolvedValue(
            new Response("png", {
                status: 200,
                headers: { "Content-Type": "image/png" },
            }),
        );
        window.fetch = fetchMock as typeof fetch;

        const { result } = renderHook(() => usePdfFileImage(fileId), {
            wrapper: createQueryClientWrapper(createTestQueryClient()),
        });

        await waitFor(() => {
            expect(result.current.isSuccess).toBe(true);
        });

        expect(fetchMock).toHaveBeenCalledWith(`/api/files/${fileId}/image`, undefined);
    });
});
