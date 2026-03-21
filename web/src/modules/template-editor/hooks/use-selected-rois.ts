import { createContext, useContext, type Dispatch } from "react";

export type SelectedRoisContextType = {
    selectedRoiIds: string[];
    setSelectedRoiIds: Dispatch<React.SetStateAction<string[]>>;
    isSelectedRoi: (roiId: string) => boolean;
};

export const SelectedRoisContext = createContext<
    SelectedRoisContextType | undefined
>(undefined);

export const useSelectedRois = (): SelectedRoisContextType => {
    const context = useContext(SelectedRoisContext);
    if (!context) {
        throw new Error(
            "useSelectedRois must be used within a SelectedRoisProvider"
        );
    }
    return context;
};
