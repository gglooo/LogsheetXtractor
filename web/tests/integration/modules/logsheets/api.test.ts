import {
    useAlignLogsheetMutation,
    useExportLogsheetsMutation,
    useLogsheetImage,
    useProcessLogsheetMutation,
    useUploadLogsheetsMutation,
} from "@/modules/logsheets/api";
import { downloadFile, fileQueryFn } from "@/modules/files/api";
import { useUserSettings } from "@/modules/settings/hooks/useUserSettings";
import { renderHook, waitFor } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";
import {
    createQueryClientWrapper,
    createTestQueryClient,
} from "../../../utils/query-client";

vi.mock("@/modules/files/api", () => ({
    fileQueryFn: vi.fn(),
    downloadFile: vi.fn(),
}));

vi.mock("@/modules/settings/hooks/useUserSettings", () => ({
    useUserSettings: vi.fn(),
}));

const originalFetch = window.fetch;

const now = new Date().toISOString();
const ids = {
    template: "11111111-1111-4111-8111-111111111111",
    file: "22222222-2222-4222-8222-222222222222",
    logsheet: "33333333-3333-4333-8333-333333333333",
    logsheet2: "66666666-6666-4666-8666-666666666666",
};

const mockUserSettings = {
    uglyCheckboxes: true,
    automaticAlignmentOnUpload: false,
};

const makeLogsheetPayload = () => ({
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
    status: "Pending",
    processedAt: null,
    alignmentData: null,
    extractedValues: [],
});

afterEach(() => {
    window.fetch = originalFetch;
    vi.restoreAllMocks();
});

describe("logsheets api hooks", () => {
    it("process mutation sends user setting options and invalidates logsheets", async () => {
        vi.mocked(useUserSettings).mockReturnValue({
            userSettings: mockUserSettings,
            setUserSettings: vi.fn(),
        });

        const fetchMock = vi
            .fn()
            .mockResolvedValue(new Response(null, { status: 200 }));
        window.fetch = fetchMock as typeof fetch;

        const queryClient = createTestQueryClient();
        const invalidateSpy = vi
            .spyOn(queryClient, "invalidateQueries")
            .mockResolvedValue(undefined);

        const { result } = renderHook(() => useProcessLogsheetMutation(), {
            wrapper: createQueryClientWrapper(queryClient),
        });

        await result.current.mutateAsync(ids.logsheet);

        const [url, init] = fetchMock.mock.calls[0];
        expect(url).toBe(`/api/logsheets/${ids.logsheet}/process`);
        expect(init?.method).toBe("POST");
        expect(init?.body).toBe(
            JSON.stringify({
                options: {
                    uglyCheckboxes: true,
                },
            }),
        );

        await waitFor(() => {
            expect(invalidateSpy).toHaveBeenCalledWith({
                queryKey: ["logsheets"],
            });
        });
    });

    it("upload mutation sends validated payload using automaticAlignmentOnUpload", async () => {
        vi.mocked(useUserSettings).mockReturnValue({
            userSettings: mockUserSettings,
            setUserSettings: vi.fn(),
        });

        const fetchMock = vi.fn().mockResolvedValue(
            new Response(JSON.stringify([makeLogsheetPayload()]), {
                status: 200,
                headers: { "Content-Type": "application/json" },
            }),
        );
        window.fetch = fetchMock as typeof fetch;

        const queryClient = createTestQueryClient();

        const { result } = renderHook(() => useUploadLogsheetsMutation(), {
            wrapper: createQueryClientWrapper(queryClient),
        });

        const response = await result.current.mutateAsync({
            templateId: ids.template,
            fileIds: [ids.file],
        });

        expect(response).toHaveLength(1);

        const [url, init] = fetchMock.mock.calls[0];
        expect(url).toBe("/api/logsheets/batch");
        expect(init?.method).toBe("POST");

        const parsedBody = JSON.parse(init?.body as string) as {
            performAutomaticAlignment: boolean;
        };
        expect(parsedBody.performAutomaticAlignment).toBe(false);
    });

    it("align mutation rounds coordinates before sending", async () => {
        const fetchMock = vi
            .fn()
            .mockResolvedValue(new Response(null, { status: 200 }));
        window.fetch = fetchMock as typeof fetch;

        const queryClient = createTestQueryClient();
        const invalidateSpy = vi
            .spyOn(queryClient, "invalidateQueries")
            .mockResolvedValue(undefined);

        const { result } = renderHook(() => useAlignLogsheetMutation(), {
            wrapper: createQueryClientWrapper(queryClient),
        });

        await result.current.mutateAsync({
            logsheetId: ids.logsheet,
            frontside: [
                { x: 10.4, y: 20.6 },
                { x: 30.9, y: 40.1 },
            ],
        });

        const [, init] = fetchMock.mock.calls[0];
        const parsedBody = JSON.parse(init?.body as string) as {
            alignment: { frontside: Array<{ x: number; y: number }> };
        };

        expect(parsedBody.alignment.frontside).toEqual([
            { x: 10, y: 21 },
            { x: 31, y: 40 },
        ]);

        await waitFor(() => {
            expect(invalidateSpy).toHaveBeenCalledWith({
                queryKey: ["logsheets"],
            });
        });
    });

    it("does not fetch logsheet image when logsheet id is missing", async () => {
        const queryClient = createTestQueryClient();

        renderHook(() => useLogsheetImage(undefined), {
            wrapper: createQueryClientWrapper(queryClient),
        });

        expect(fileQueryFn).not.toHaveBeenCalled();
    });

    it("export logsheets mutation uses fileQueryFn and downloadFile", async () => {
        vi.mocked(fileQueryFn).mockResolvedValue({
            bytes: new ArrayBuffer(16),
            fileName: "export.zip",
            contentType: "application/zip",
        });

        const queryClient = createTestQueryClient();
        const { result } = renderHook(() => useExportLogsheetsMutation(), {
            wrapper: createQueryClientWrapper(queryClient),
        });

        await result.current.mutateAsync([ids.logsheet, ids.logsheet2]);

        expect(fileQueryFn).toHaveBeenCalledWith("/api/logsheets/batch/export", {
            method: "POST",
            body: JSON.stringify({
                logsheetIds: [ids.logsheet, ids.logsheet2],
            }),
            headers: {
                "Content-Type": "application/json",
            },
        });

        expect(downloadFile).toHaveBeenCalledTimes(1);
    });
});
