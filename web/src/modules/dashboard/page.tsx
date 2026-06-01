import { Spinner } from "@/components/ui/spinner";
import { TemplateListItem } from "@/modules/dashboard/components/template-list-item";
import { useTemplates } from "@/modules/templates/api";
import { useIntl } from "react-intl";

export const DashboardPage = () => {
    const templates = useTemplates();

    const intl = useIntl();

    return (
        <main className="p-10">
            {templates.isLoading ? (
                <div className="flex flex-1 items-center justify-center mb-10">
                    <Spinner className="self-center" />
                </div>
            ) : null}
            {templates.isError ? (
                <div className="text-red-500">
                    {intl.formatMessage({
                        id: "dashboard.errorLoadingTemplates",
                        defaultMessage: "Error loading templates.",
                    })}
                </div>
            ) : null}
            <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 2xl:grid-cols-4 gap-4">
                {templates.data?.map((template) => (
                    <TemplateListItem key={template.id} template={template} />
                ))}
                {templates.data && templates.data.length === 0 ? (
                    <div>
                        {intl.formatMessage({
                            id: "dashboard.noTemplates",
                            defaultMessage: "No templates available.",
                        })}
                    </div>
                ) : null}
            </div>
        </main>
    );
};
