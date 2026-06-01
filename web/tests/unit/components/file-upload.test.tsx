import { FileUpload } from "@/components/file-upload";
import { renderWithProviders } from "../../utils/render-with-providers";
import { fireEvent, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

beforeEach(() => {
    localStorage.setItem("app-locale", "en");
});

describe("FileUpload", () => {
    it("ignores dropped files that fail the validator", () => {
        const onFileChange = vi.fn();
        const { container } = renderWithProviders(
            <FileUpload
                className="upload-zone"
                file={null}
                onFileChange={onFileChange}
                validator={(file) => file.type === "application/pdf"}
            />,
        );

        fireEvent.drop(container.querySelector(".upload-zone")!, {
            dataTransfer: {
                files: [
                    new File(["{}"], "config.json", {
                        type: "application/json",
                    }),
                ],
            },
        });

        expect(onFileChange).not.toHaveBeenCalled();
    });

    it("drops only valid files in multi-file mode", () => {
        const onFileChange = vi.fn();
        const pdf = new File(["%PDF"], "template.pdf", {
            type: "application/pdf",
        });
        const json = new File(["{}"], "config.json", {
            type: "application/json",
        });

        const { container } = renderWithProviders(
            <FileUpload
                className="upload-zone"
                file={null}
                onFileChange={onFileChange}
                multiple
                validator={(file) => file.type === "application/pdf"}
            />,
        );

        fireEvent.drop(container.querySelector(".upload-zone")!, {
            dataTransfer: {
                files: [pdf, json],
            },
        });

        expect(onFileChange).toHaveBeenCalledWith([pdf]);
    });

    it("removes one file from a multi-file selection", async () => {
        const user = userEvent.setup();
        const onFileChange = vi.fn();
        const keep = new File(["%PDF"], "keep.pdf", {
            type: "application/pdf",
        });
        const remove = new File(["%PDF"], "remove.pdf", {
            type: "application/pdf",
        });

        renderWithProviders(
            <FileUpload
                file={[keep, remove]}
                onFileChange={onFileChange}
                multiple
            />,
        );

        const removeButtons = screen.getAllByRole("button", {
            name: "",
        });
        await user.click(removeButtons[1]);

        expect(onFileChange).toHaveBeenCalledWith([keep]);
    });
});
