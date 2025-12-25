import { Skeleton } from "@/components/ui/skeleton";
import { useFile } from "@/modules/files/api";
import { usePdfZoom } from "@/modules/pdf/context/pdf-zoom-context";
import { useMemo } from "react";
import { Document, Page } from "react-pdf";
import "react-pdf/dist/Page/AnnotationLayer.css";
import "react-pdf/dist/Page/TextLayer.css";

const A4_ASPECT_RATIO = 1.414;

export const PdfViewer = ({ fileId }: { fileId: string }) => {
    const file = useFile(fileId);
    const fileData = useMemo(() => {
        return file.data?.bytes
            ? { data: new Uint8Array(file.data.bytes.slice(0)) }
            : undefined;
    }, [file.data]);

    const { scale, width } = usePdfZoom();

    return !file.isPending && fileData ? (
        <Document
            file={fileData}
            onLoadError={(error) =>
                console.error("Error loading PDF Document:", error)
            }
            onSourceError={(error) =>
                console.error("Error loading PDF Source:", error)
            }
            className="select-none"
        >
            <Page
                pageNumber={1}
                width={width * scale}
                onLoadError={(error) =>
                    console.error("Error loading PDF Page:", error)
                }
            />
        </Document>
    ) : (
        <Skeleton
            style={{
                width: width * scale,
                height: width * scale * A4_ASPECT_RATIO,
            }}
        />
    );
};
