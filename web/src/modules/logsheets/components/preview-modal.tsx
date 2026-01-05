import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";
import { PdfViewer } from "@/modules/pdf/components/pdf-viewer";
import { PdfWrapper } from "@/modules/pdf/components/pdf-wrapper";
import { useIntl } from "react-intl";

type Props = {
    isOpen: boolean;
    onClose: () => void;
    fileId: string | null;
};

export const PreviewModal = ({ isOpen, onClose, fileId }: Props) => {
    const intl = useIntl();
    return (
        <Dialog open={isOpen} onOpenChange={onClose}>
            <DialogContent className="max-h-[90vh] min-w-[90vw] flex flex-col">
                <DialogHeader>
                    <DialogTitle>
                        {intl.formatMessage({
                            id: "logsheets.preview.title",
                            defaultMessage: "Logsheet preview",
                        })}
                    </DialogTitle>
                </DialogHeader>
                <div className="flex-1 overflow-y-auto min-h-0">
                    <PdfWrapper
                        includeHistoryControls={false}
                        includeZoomControls={false}
                    >
                        {fileId ? <PdfViewer fileId={fileId} /> : null}
                    </PdfWrapper>
                </div>
            </DialogContent>
        </Dialog>
    );
};
