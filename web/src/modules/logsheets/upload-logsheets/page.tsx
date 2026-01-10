import { PdfFileUpload } from "@/components/pdf-file-upload";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { TemplateInfoItem } from "@/modules/logsheets/upload-logsheets/components/template-info-item";
import { useUploadLogsheetsContext } from "@/modules/logsheets/upload-logsheets/hooks/use-upload-logsheets-context";
import { useTemplate } from "@/modules/templates/api";
import { formatDate } from "date-fns";
import { useIntl } from "react-intl";
import { useParams } from "react-router-dom";

export const UploadLogsheetPage = () => {
    const intl = useIntl();

    const { templateId } = useParams<{ templateId: string }>();
    const templateQuery = useTemplate(templateId ?? "");

    const { files, setFiles } = useUploadLogsheetsContext();

    const handleFileChange = (file: File | File[] | null) => {
        if (!file) {
            setFiles([]);
            return;
        }

        if (Array.isArray(file)) {
            setFiles(file);
        } else {
            setFiles([file]);
        }
    };

    if (!templateId) {
        return (
            <p>
                {intl.formatMessage({
                    id: "error.template-id-required",
                    defaultMessage: "Template ID is required.",
                })}
            </p>
        );
    }

    if (templateQuery.isError) {
        return (
            <p className="p-4">
                {intl.formatMessage({
                    id: "error.loading-template",
                    defaultMessage: "Error loading template.",
                })}
            </p>
        );
    }

    return (
        <div className="flex flex-col items-center overflow-hidden w-full pt-10 pb-6 bg-background">
            <div className="flex flex-col items-center gap-6 w-10/12 py-20 md:w-9/12 lg:w-8/12">
                <PdfFileUpload
                    file={files}
                    onFileChange={handleFileChange}
                    multiple
                    className="w-full bg-white dark:bg-muted"
                />

                <Card className="w-full">
                    <CardHeader>
                        <CardTitle>
                            {intl.formatMessage({
                                id: "logsheets.upload.template-info",
                                defaultMessage: "Template information",
                            })}
                        </CardTitle>
                    </CardHeader>
                    <CardContent>
                        <div className="grid grid-cols-2 md:grid-cols-3 gap-4 text-sm">
                            <TemplateInfoItem
                                label={intl.formatMessage({
                                    id: "logsheets.upload.template-info.name",
                                    defaultMessage: "Template name",
                                })}
                                value={templateQuery.data?.name}
                                loading={templateQuery.isLoading}
                            />
                            <TemplateInfoItem
                                label={intl.formatMessage({
                                    id: "logsheets.upload.template-info.defined-rois",
                                    defaultMessage: "Defined fields (ROIs)",
                                })}
                                value={templateQuery.data?.rois.length.toString()}
                                loading={templateQuery.isLoading}
                            />
                            <TemplateInfoItem
                                label={intl.formatMessage({
                                    id: "logsheets.upload.template-info.last-updated",
                                    defaultMessage: "Last updated",
                                })}
                                value={
                                    templateQuery.data?.updatedAt
                                        ? formatDate(
                                              new Date(
                                                  templateQuery.data.updatedAt
                                              ),
                                              "PP p"
                                          )
                                        : "-"
                                }
                                loading={templateQuery.isLoading}
                            />
                        </div>
                    </CardContent>
                </Card>
            </div>
        </div>
    );
};
