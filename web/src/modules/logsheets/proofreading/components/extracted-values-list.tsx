import type { ExtractedValueType } from "@/modules/logsheets/schema";
import { ExtractedValueCard } from "./extracted-value-card";

interface ExtractedValuesListProps {
    extractedValues: ExtractedValueType[];
}

export const ExtractedValuesList = ({
    extractedValues,
}: ExtractedValuesListProps) => {
    return (
        <div className="flex flex-col gap-4 p-4 h-full overflow-y-auto">
            {extractedValues.map((value) => (
                <ExtractedValueCard key={value.id} extractedValue={value} />
            ))}
        </div>
    );
};
