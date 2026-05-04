import { Skeleton } from "@/components/ui/skeleton";
import { useSvgZoom } from "@/modules/canvas/context/svg-zoom-context";
import { useFile } from "@/modules/files/api";
import { useMemo, useState } from "react";
import { useIntl } from "react-intl";
import { Document, Page } from "react-pdf";
import "react-pdf/dist/Page/AnnotationLayer.css";
import "react-pdf/dist/Page/TextLayer.css";

const A4_ASPECT_RATIO = 1.414;
const USABLE_WIDTH_THRESHOLD = 100;

const PdfSkeleton = ({ width, height }: { width: number; height: number }) => (
    <Skeleton
        style={{
            width,
            height,
        }}
    />
);

type PdfFileData = {
    data: Uint8Array;
};

export const PdfViewer = ({
    fileId,
    pageNumber,
    onNumPagesLoaded,
}: {
    fileId: string;
    pageNumber?: number;
    onNumPagesLoaded?: (numPages: number) => void;
}) => {
    const intl = useIntl();
    const file = useFile(fileId);
    const fileBytes = file.data?.bytes;
    const fileData = useMemo<PdfFileData | undefined>(() => {
        return fileBytes
            ? { data: new Uint8Array(fileBytes.slice(0)) }
            : undefined;
    }, [fileBytes]);

    const { scale, width } = useSvgZoom();

    const [loadedDocument, setLoadedDocument] = useState<{
        fileId: string;
        numPages: number;
    } | null>(null);
    const numPages =
        loadedDocument?.fileId === fileId ? loadedDocument.numPages : null;

    const onDocumentLoadSuccess = ({ numPages }: { numPages: number }) => {
        setLoadedDocument({ fileId, numPages });
        onNumPagesLoaded?.(numPages);
    };

    const renderedPages = useMemo(() => {
        if (numPages === null) {
            return null;
        }

        if (pageNumber) {
            return pageNumber > numPages ? (
                <div
                    className="flex flex-col items-center justify-center p-8 text-center text-muted-foreground bg-muted/20 border border-dashed rounded-md absolute inset-0 z-20 backdrop-blur-sm"
                    style={{
                        width: width * scale,
                        height: width * scale * A4_ASPECT_RATIO,
                    }}
                >
                    <p className="font-medium text-lg mb-1">
                        {intl.formatMessage({
                            id: "pdf.pageNotFound.title",
                            defaultMessage: "Page Not Found",
                        })}
                    </p>
                    <p className="text-sm">
                        {intl.formatMessage(
                            {
                                id: "pdf.pageNotFound.description",
                                defaultMessage:
                                    "This document only has {numPages} {numPages, plural, one {page} other {pages}}, but page {pageNumber} was requested.",
                            },
                            { numPages, pageNumber },
                        )}
                    </p>
                </div>
            ) : (
                <Page
                    pageNumber={pageNumber}
                    width={width * scale}
                    onLoadError={(error) =>
                        console.error("Error loading PDF Page:", error)
                    }
                />
            );
        }

        return Array.from({ length: numPages }, (_, index) => (
            <Page
                key={`page_${index}`}
                pageNumber={index + 1}
                width={width * scale}
                onLoadError={(error) =>
                    console.error("Error loading PDF Page:", error)
                }
            />
        ));
    }, [intl, numPages, pageNumber, scale, width]);

    return !file.isPending && fileData && width > USABLE_WIDTH_THRESHOLD ? (
        <Document
            key={fileId}
            file={fileData}
            loading={
                <PdfSkeleton
                    width={width * scale}
                    height={width * scale * A4_ASPECT_RATIO}
                />
            }
            onLoadSuccess={onDocumentLoadSuccess}
            onLoadError={(error) =>
                console.error("Error loading PDF Document:", error)
            }
            onSourceError={(error) =>
                console.error("Error loading PDF Source:", error)
            }
            className="select-none"
        >
            {renderedPages}
        </Document>
    ) : (
        <PdfSkeleton
            width={width * scale}
            height={width * scale * A4_ASPECT_RATIO}
        />
    );
};
