import { Skeleton } from "@/components/ui/skeleton";
import { useFile } from "@/modules/files/api";
import { usePdfZoom } from "@/modules/pdf/context/pdf-zoom-context";
import { useMemo } from "react";
import { Document, Page, pdfjs } from "react-pdf";

pdfjs.GlobalWorkerOptions.workerSrc = new URL(
    "pdfjs-dist/build/pdf.worker.min.mjs",
    import.meta.url
).toString();

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
        <Document file={fileData}>
            <Page pageNumber={1} width={width * scale} />
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
