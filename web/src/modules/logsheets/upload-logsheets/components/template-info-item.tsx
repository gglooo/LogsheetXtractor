import { Skeleton } from "@/components/ui/skeleton";

export const TemplateInfoItem = ({
    label,
    value,
    loading,
}: {
    label: string;
    value: string | number | undefined;
    loading?: boolean;
}) => {
    return (
        <div className="min-w-0">
            <p className="text-muted-foreground">{label}</p>
            {loading ? (
                <Skeleton className="w-32 h-6" />
            ) : (
                <p className="font-medium text-foreground break-words hyphens-auto">
                    {value}
                </p>
            )}
        </div>
    );
};
