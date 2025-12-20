import { SelectedRoisContext } from "@/modules/template-editor/hooks/use-selected-rois";
import { useMemo, useState, type PropsWithChildren } from "react";

export const SelectedRoisProvider = ({ children }: PropsWithChildren) => {
    const [selectedRoiIds, setSelectedRoiIds] = useState<string[]>([]);

    const selectedRoiIdsSet = useMemo(
        () => new Set(selectedRoiIds),
        [selectedRoiIds]
    );

    const isSelectedRoi = (roiId: string) => {
        return selectedRoiIdsSet.has(roiId);
    };

    return (
        <SelectedRoisContext.Provider
            value={{
                selectedRoiIds,
                setSelectedRoiIds,
                isSelectedRoi,
            }}
        >
            {children}
        </SelectedRoisContext.Provider>
    );
};
