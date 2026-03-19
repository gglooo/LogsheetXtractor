import { roiValidationRuleCatalogSchema } from "@/modules/rois/validation/schema";
import { useQuery } from "@tanstack/react-query";

export const useRoiValidationRuleCatalog = () =>
    useQuery({
        queryKey: ["roi-validation-rules"],
        queryFn: async () => {
            const response = await fetch("/api/roi-validation/rules");
            return await roiValidationRuleCatalogSchema.parseAsync(
                await response.json(),
            );
        },
    });
