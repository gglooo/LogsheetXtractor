import { copy, paste } from "@/modules/template-editor/utils/copy-paste";
import { afterAll, afterEach, describe, expect, it, vi } from "vitest";

const clipboard = {
    readText: vi.fn(),
    writeText: vi.fn(),
};

const originalClipboard = navigator.clipboard;

Object.defineProperty(navigator, "clipboard", {
    configurable: true,
    value: clipboard,
});

afterEach(() => {
    vi.clearAllMocks();
    Object.defineProperty(navigator, "clipboard", {
        configurable: true,
        value: clipboard,
    });
});

afterAll(() => {
    Object.defineProperty(navigator, "clipboard", {
        configurable: true,
        value: originalClipboard,
    });
});

describe("template-editor copy/paste utils", () => {
    it("serializes copied ROI payloads to the clipboard", async () => {
        clipboard.writeText.mockResolvedValue(undefined);
        const rois = [
            {
                id: "11111111-1111-4111-8111-111111111111",
                variableName: "field",
                coordinates: { x: 10, y: 20, width: 30, height: 40 },
            },
        ];

        await copy(rois);

        expect(clipboard.writeText).toHaveBeenCalledWith(JSON.stringify(rois));
    });

    it("parses pasted ROI payloads from the clipboard", async () => {
        const rois = [
            {
                id: "11111111-1111-4111-8111-111111111111",
                variableName: "field_copy",
                coordinates: { x: 90, y: 100, width: 30, height: 40 },
            },
        ];
        clipboard.readText.mockResolvedValue(JSON.stringify(rois));

        await expect(paste()).resolves.toEqual(rois);
    });

    it("returns null and reports the parse error when pasted content is invalid", async () => {
        const onError = vi.fn();
        clipboard.readText.mockResolvedValue("not json");

        await expect(paste(onError)).resolves.toBeNull();

        expect(onError).toHaveBeenCalledOnce();
    });
});
