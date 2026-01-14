import { Skeleton } from "@/components/ui/skeleton";
import { useFile } from "@/modules/files/api";
import { usePdfZoom } from "@/modules/pdf/context/pdf-zoom-context";
import { useMemo, useState } from "react";
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

export const PdfViewer = ({
    fileId,
    pageNumber,
}: {
    fileId: string;
    pageNumber?: number;
}) => {
    const file = useFile(fileId);
    const fileData = useMemo(() => {
        return file.data?.bytes
            ? { data: new Uint8Array(file.data.bytes.slice(0)) }
            : undefined;
    }, [file.data]);

    const { scale, width } = usePdfZoom();

    const [numPages, setNumPages] = useState<number | null>(null);

    const onDocumentLoadSuccess = ({ numPages }: { numPages: number }) => {
        setNumPages(numPages);
    };

    return !file.isPending && fileData && width > USABLE_WIDTH_THRESHOLD ? (
        <Document
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
            {pageNumber ? (
                <Page
                    pageNumber={pageNumber}
                    width={width * scale}
                    onLoadError={(error) =>
                        console.error("Error loading PDF Page:", error)
                    }
                />
            ) : (
                Object.entries(Array(numPages).fill(null)).map(([index]) => (
                    <Page
                        key={`page_${index}`}
                        pageNumber={Number(index) + 1}
                        width={width * scale}
                        onLoadError={(error) =>
                            console.error("Error loading PDF Page:", error)
                        }
                    />
                ))
            )}
        </Document>
    ) : (
        <PdfSkeleton
            width={width * scale}
            height={width * scale * A4_ASPECT_RATIO}
        />
    );
};
