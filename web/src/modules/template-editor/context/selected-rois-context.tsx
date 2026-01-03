import { SelectedRoisContext } from "@/modules/template-editor/hooks/use-selected-rois";
import { useCallback, useMemo, useState, type PropsWithChildren } from "react";

export const SelectedRoisProvider = ({ children }: PropsWithChildren) => {
    const [selectedRoiIds, setSelectedRoiIds_] = useState<string[]>([]);

    const selectedRoiIdsSet = useMemo(
        () => new Set(selectedRoiIds),
        [selectedRoiIds]
    );

    const isSelectedRoi = useCallback(
        (roiId: string) => {
            return selectedRoiIdsSet.has(roiId);
        },
        [selectedRoiIdsSet]
    );

    const setSelectedRoiIds = useCallback(
        (ids: string[] | ((prevIds: string[]) => string[])) => {
            console.log("Setting selected ROI IDs:", ids);

            setSelectedRoiIds_(ids);
        },
        []
    );

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
