import { I18nProvider } from "@/components/i18n-provider";
import { SvgZoomContext } from "@/modules/canvas/context/svg-zoom-context";
import { PdfViewer } from "@/modules/pdf/components/pdf-viewer";
import { render, screen } from "@testing-library/react";
import { useEffect, type ReactNode } from "react";
import { beforeEach, describe, expect, it, vi } from "vitest";

const useFileMock = vi.hoisted(() => vi.fn());

vi.mock("@/modules/files/api", () => ({
    useFile: useFileMock,
}));

vi.mock("react-pdf", () => ({
    Document: ({
        children,
        onLoadSuccess,
    }: {
        children: ReactNode;
        onLoadSuccess: (document: { numPages: number }) => void;
    }) => {
        useEffect(() => {
            onLoadSuccess({ numPages: 2 });
        }, [onLoadSuccess]);

        return <div data-testid="pdf-document">{children}</div>;
    },
    Page: ({ pageNumber, width }: { pageNumber: number; width: number }) => (
        <div data-testid="pdf-page" data-page-number={pageNumber} data-width={width} />
    ),
}));

const renderViewer = (
    ui: ReactNode,
    zoom: { width: number; scale: number } = { width: 500, scale: 1 },
) =>
    render(
        <I18nProvider>
            <SvgZoomContext.Provider value={zoom}>{ui}</SvgZoomContext.Provider>
        </I18nProvider>,
    );

describe("PdfViewer", () => {
    beforeEach(() => {
        useFileMock.mockReturnValue({
            isPending: false,
            data: { bytes: new Uint8Array([1, 2, 3]).buffer },
        });
    });

    it("renders all PDF pages once the document reports its page count", async () => {
        const onNumPagesLoaded = vi.fn();

        renderViewer(
            <PdfViewer fileId="file-1" onNumPagesLoaded={onNumPagesLoaded} />,
        );

        expect(await screen.findByTestId("pdf-document")).toBeInTheDocument();
        expect(await screen.findAllByTestId("pdf-page")).toHaveLength(2);
        expect(screen.getAllByTestId("pdf-page")[0]).toHaveAttribute(
            "data-width",
            "500",
        );
        expect(onNumPagesLoaded).toHaveBeenCalledWith(2);
    });

    it("renders only the requested page when it exists", async () => {
        renderViewer(<PdfViewer fileId="file-1" pageNumber={2} />);

        const page = await screen.findByTestId("pdf-page");

        expect(page).toHaveAttribute("data-page-number", "2");
        expect(screen.getAllByTestId("pdf-page")).toHaveLength(1);
    });

    it("shows a page-not-found state when the requested page exceeds the document", async () => {
        renderViewer(<PdfViewer fileId="file-1" pageNumber={3} />);

        expect(await screen.findByText("Page Not Found")).toBeInTheDocument();
        expect(
            screen.getByText(
                "This document only has 2 pages, but page 3 was requested.",
            ),
        ).toBeInTheDocument();
        expect(screen.queryByTestId("pdf-page")).not.toBeInTheDocument();
    });

    it("keeps the skeleton visible while file bytes or usable width are unavailable", () => {
        useFileMock.mockReturnValue({ isPending: true, data: undefined });

        renderViewer(<PdfViewer fileId="file-1" />, { width: 80, scale: 1 });

        expect(screen.queryByTestId("pdf-document")).not.toBeInTheDocument();
    });
});
