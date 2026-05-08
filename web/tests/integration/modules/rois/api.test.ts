import {
    useDetectRoisMutation,
    useSetRoisMutation,
} from "@/modules/rois/api";
import { renderHook, waitFor } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";
import {
    createQueryClientWrapper,
    createTestQueryClient,
} from "../../../utils/query-client";

const originalFetch = window.fetch;

const now = new Date().toISOString();
const templateId = "11111111-1111-4111-8111-111111111111";
const roiId = "22222222-2222-4222-8222-222222222222";

afterEach(() => {
    window.fetch = originalFetch;
    vi.restoreAllMocks();
});

describe("rois api hooks", () => {
    it("detect-rois mutation posts to endpoint and invalidates rois key", async () => {
        const fetchMock = vi.fn().mockResolvedValue(
            new Response(
                JSON.stringify({
                    rois: [
                        {
                            id: null,
                            createdAt: now,
                            updatedAt: null,
                            deletedAt: null,
                            variableName: "detectedRoi",
                            templateId,
                            type: "Handwritten",
                            coordinates: {
                                x: 10,
                                y: 20,
                                width: 100,
                                height: 40,
                            },
                            validationCondition: null,
                        },
                    ],
                    residuals: [],
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

        const { result } = renderHook(() => useDetectRoisMutation(), {
            wrapper: createQueryClientWrapper(queryClient),
        });

        const response = await result.current.mutateAsync(templateId);

        expect(response.rois).toHaveLength(1);
        expect(fetchMock).toHaveBeenCalledWith(
            `/api/templates/${templateId}/detect-rois`,
            {
                method: "POST",
            },
        );

        await waitFor(() => {
            expect(invalidateSpy).toHaveBeenCalledWith({
                queryKey: ["rois"],
            });
        });
    });

    it("set-rois mutation sends validated payload and invalidates dependent keys", async () => {
        const fetchMock = vi.fn().mockResolvedValue(
            new Response(
                JSON.stringify([
                    {
                        id: roiId,
                        createdAt: now,
                        updatedAt: null,
                        deletedAt: null,
                        variableName: "existingRoi",
                        templateId,
                        type: "Number",
                        coordinates: {
                            x: 1,
                            y: 2,
                            width: 3,
                            height: 4,
                        },
                        validationCondition: null,
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
        const invalidateSpy = vi
            .spyOn(queryClient, "invalidateQueries")
            .mockResolvedValue(undefined);

        const { result } = renderHook(() => useSetRoisMutation(templateId), {
            wrapper: createQueryClientWrapper(queryClient),
        });

        await result.current.mutateAsync({
            rois: [
                {
                    id: null,
                    createdAt: now,
                    updatedAt: null,
                    deletedAt: null,
                    variableName: "newRoi",
                    templateId,
                    type: null,
                    coordinates: {
                        x: 10,
                        y: 20,
                        width: 30,
                        height: 40,
                    },
                    validationCondition: null,
                },
            ],
        });

        const [calledUrl, calledInit] = fetchMock.mock.calls[0];
        expect(calledUrl).toBe(`/api/templates/${templateId}/rois/set`);
        expect(calledInit?.method).toBe("POST");
        expect(calledInit?.headers).toEqual({
            "Content-Type": "application/json",
        });

        const parsedBody = JSON.parse(calledInit?.body as string) as {
            rois: Array<{ variableName: string }>;
        };
        expect(parsedBody.rois[0].variableName).toBe("newRoi");

        await waitFor(() => {
            expect(invalidateSpy).toHaveBeenCalledWith({
                queryKey: ["rois"],
            });
            expect(invalidateSpy).toHaveBeenCalledWith({
                queryKey: ["template", templateId],
            });
        });
    });
});
