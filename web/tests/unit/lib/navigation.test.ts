import { describe, expect, it } from "vitest";
import { resolveParentPath } from "@/lib/navigation";

describe("resolveParentPath", () => {
    it("resolves template editor parent", () => {
        expect(resolveParentPath("/template-editor/abc")).toBe("/dashboard");
    });

    it("resolves logsheet upload parent with template id", () => {
        expect(resolveParentPath("/templates/template-1/logsheets/upload")).toBe(
            "/templates/template-1/logsheets",
        );
    });

    it("resolves align/proofread parent with template id", () => {
        expect(
            resolveParentPath("/templates/t-1/logsheets/l-1/align"),
        ).toBe("/templates/t-1/logsheets");

        expect(
            resolveParentPath("/templates/t-1/logsheets/l-1/proofread"),
        ).toBe("/templates/t-1/logsheets");
    });

    it("returns undefined when no mapping exists", () => {
        expect(resolveParentPath("/unknown/path")).toBeUndefined();
    });
});
