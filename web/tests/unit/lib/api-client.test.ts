import {
    ApiValidationError,
    setupGlobalFetchInterceptor,
} from "@/lib/api-client";
import { afterEach, describe, expect, it, vi } from "vitest";

const originalFetch = window.fetch;

afterEach(() => {
    window.fetch = originalFetch;
    vi.restoreAllMocks();
});

describe("api-client fetch interceptor", () => {
    it("passes through successful responses", async () => {
        const mockResponse = new Response(JSON.stringify({ ok: true }), {
            status: 200,
            headers: { "Content-Type": "application/json" },
        });

        window.fetch = vi.fn().mockResolvedValue(mockResponse);
        setupGlobalFetchInterceptor();

        const response = await window.fetch("/api/test");

        expect(response).toBe(mockResponse);
    });

    it("throws ApiValidationError for validation payload", async () => {
        const payload = {
            code: "VALIDATION_ERROR",
            detail: "Validation failed",
            errorMessages: ["field is required"],
        };

        const mockResponse = new Response(JSON.stringify(payload), {
            status: 400,
            headers: { "Content-Type": "application/json" },
        });

        window.fetch = vi.fn().mockResolvedValue(mockResponse);
        setupGlobalFetchInterceptor();

        await expect(window.fetch("/api/test")).rejects.toBeInstanceOf(
            ApiValidationError,
        );

        await expect(window.fetch("/api/test")).rejects.toMatchObject({
            message: "Validation failed",
            errorMessages: ["field is required"],
        });
    });

    it("does not throw ApiValidationError for non-json non-ok response", async () => {
        const mockResponse = new Response("Service unavailable", {
            status: 503,
            headers: { "Content-Type": "text/plain" },
        });

        window.fetch = vi.fn().mockResolvedValue(mockResponse);
        setupGlobalFetchInterceptor();

        const response = await window.fetch("/api/test");

        expect(response.status).toBe(503);
    });
});
