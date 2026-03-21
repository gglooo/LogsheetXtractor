import {
    useAddTemplateBacksideMutation,
    useCreateTemplateMutation,
    useExportConfigMutation,
    useTemplates,
} from "@/modules/templates/api";
import {
    downloadFile,
    fileQueryFn,
    useUploadFileMutation,
} from "@/modules/files/api";
import { renderHook, waitFor } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";
import {
    createQueryClientWrapper,
    createTestQueryClient,
} from "../../../utils/query-client";

vi.mock("@/modules/files/api", () => ({
    fileQueryFn: vi.fn(),
    downloadFile: vi.fn(),
    useUploadFileMutation: vi.fn(),
}));

const originalFetch = window.fetch;

const now = new Date().toISOString();
const ids = {
    template: "11111111-1111-4111-8111-111111111111",
    file: "22222222-2222-4222-8222-222222222222",
    uploadedFront: "33333333-3333-4333-8333-333333333333",
    uploadedBack: "44444444-4444-4444-8444-444444444444",
};

const makeTemplatePayload = () => ({
    id: ids.template,
    createdAt: now,
    updatedAt: null,
    deletedAt: null,
    name: "Template A",
    parent: null,
    width: 100,
    height: 200,
    file: {
        id: ids.file,
        createdAt: now,
        updatedAt: null,
        deletedAt: null,
        fileName: "template.pdf",
        contentType: "application/pdf",
        sizeBytes: 10,
    },
    rois: [],
    residuals: [],
    isEditable: true,
    frontsideTemplate: null,
    backsideTemplate: null,
});

afterEach(() => {
    window.fetch = originalFetch;
    vi.restoreAllMocks();
});

describe("templates api hooks", () => {
    it("fetches and parses templates list", async () => {
        const fetchMock = vi.fn().mockResolvedValue(
            new Response(
                JSON.stringify([
                    {
                        id: ids.template,
                        createdAt: now,
                        updatedAt: null,
                        deletedAt: null,
                        name: "Template A",
                        parentId: null,
                        backsideTemplateId: null,
                        fileId: ids.file,
                        roiCount: 0,
                        logsheetCount: 0,
                        width: 100,
                        height: 200,
                    },
                ]),
                {
                    status: 200,
                    headers: { "Content-Type": "application/json" },
                },
            ),
        );
        window.fetch = fetchMock as typeof fetch;

        const queryClient = createTestQueryClient();
        const { result } = renderHook(() => useTemplates(), {
            wrapper: createQueryClientWrapper(queryClient),
        });

        await waitFor(() => {
            expect(result.current.isSuccess).toBe(true);
        });

        expect(result.current.data).toHaveLength(1);
        expect(fetchMock).toHaveBeenCalledWith("/api/templates");
    });

    it("add-backside mutation invalidates templates and template detail keys", async () => {
        const fetchMock = vi.fn().mockResolvedValue(
            new Response(JSON.stringify(makeTemplatePayload()), {
                status: 200,
                headers: { "Content-Type": "application/json" },
            }),
        );
        window.fetch = fetchMock as typeof fetch;

        const queryClient = createTestQueryClient();
        const invalidateSpy = vi
            .spyOn(queryClient, "invalidateQueries")
            .mockResolvedValue(undefined);

        const { result } = renderHook(() => useAddTemplateBacksideMutation(), {
            wrapper: createQueryClientWrapper(queryClient),
        });

        await result.current.mutateAsync({
            templateId: ids.template,
            name: "Backside",
            fileId: ids.file,
        });

        await waitFor(() => {
            expect(invalidateSpy).toHaveBeenCalledWith({
                queryKey: ["templates"],
            });
            expect(invalidateSpy).toHaveBeenCalledWith({
                queryKey: ["template", ids.template],
            });
        });
    });

    it("create-template mutation uploads files, posts payload, and invalidates templates", async () => {
        const mutateAsyncMock = vi
            .fn()
            .mockResolvedValueOnce({ id: ids.uploadedFront })
            .mockResolvedValueOnce({ id: ids.uploadedBack });

        vi.mocked(useUploadFileMutation).mockReturnValue({
            mutateAsync: mutateAsyncMock,
        } as unknown as ReturnType<typeof useUploadFileMutation>);

        const fetchMock = vi.fn().mockResolvedValue(
            new Response(JSON.stringify(makeTemplatePayload()), {
                status: 200,
                headers: { "Content-Type": "application/json" },
            }),
        );
        window.fetch = fetchMock as typeof fetch;

        const queryClient = createTestQueryClient();
        const invalidateSpy = vi
            .spyOn(queryClient, "invalidateQueries")
            .mockResolvedValue(undefined);

        const { result } = renderHook(() => useCreateTemplateMutation(), {
            wrapper: createQueryClientWrapper(queryClient),
        });

        const frontConfig = {
            text: vi.fn().mockResolvedValue("{\"a\":1}"),
        } as unknown as File;
        const backConfig = {
            text: vi.fn().mockResolvedValue("{\"b\":2}"),
        } as unknown as File;

        await result.current.mutateAsync({
            name: "Template A",
            file: new File(["%PDF"], "front.pdf", {
                type: "application/pdf",
            }),
            importedConfig: frontConfig,
            backside: {
                name: "Template A Back",
                file: new File(["%PDF"], "back.pdf", {
                    type: "application/pdf",
                }),
                importedConfig: backConfig,
            },
        });

        expect(mutateAsyncMock).toHaveBeenCalledTimes(2);

        const [, init] = fetchMock.mock.calls[0];
        const parsedBody = JSON.parse(init?.body as string) as {
            fileId: string;
            backside: { fileId: string; importedConfig: string };
            importedConfig: string;
        };

        expect(parsedBody.fileId).toBe(ids.uploadedFront);
        expect(parsedBody.backside.fileId).toBe(ids.uploadedBack);
        expect(parsedBody.importedConfig).toBe("{\"a\":1}");
        expect(parsedBody.backside.importedConfig).toBe("{\"b\":2}");

        await waitFor(() => {
            expect(invalidateSpy).toHaveBeenCalledWith({
                queryKey: ["templates"],
            });
        });
    });

    it("export-config mutation delegates to fileQueryFn and downloadFile", async () => {
        vi.mocked(fileQueryFn).mockResolvedValue({
            bytes: new Uint8Array([1, 2, 3]).buffer,
            fileName: "config.json",
            contentType: "application/json",
        });

        const queryClient = createTestQueryClient();
        const { result } = renderHook(() => useExportConfigMutation(), {
            wrapper: createQueryClientWrapper(queryClient),
        });

        await result.current.mutateAsync({ templateId: ids.template });

        expect(fileQueryFn).toHaveBeenCalledWith(
            `api/templates/${ids.template}/export-config`,
            { method: "POST" },
        );
        expect(downloadFile).toHaveBeenCalledTimes(1);
    });
});
