import { useLogsheet } from "@/modules/logsheets/api";
import type { ExtractedValueType } from "@/modules/logsheets/schema";
import { useMemo } from "react";

export const useExtractedValues = (logsheetId: string) => {
    const { data: logsheet, isLoading: isLogsheetLoading } =
        useLogsheet(logsheetId);

    const { unverifiedExtractedValuesMap, verifiedExtractedValuesMap } =
        useMemo(() => {
            const unverified: Record<string, ExtractedValueType> = {};
            const verified: Record<string, ExtractedValueType> = {};

            logsheet?.extractedValues.forEach((ev) => {
                if (ev.status === "Unverified") {
                    unverified[ev.roiId] = ev;
                } else {
                    verified[ev.roiId] = ev;
                }
            });

            return {
                unverifiedExtractedValuesMap: unverified,
                verifiedExtractedValuesMap: verified,
            };
        }, [logsheet]);

    const unverifiedExtractedValues = useMemo(() => {
        return Object.values(unverifiedExtractedValuesMap);
    }, [unverifiedExtractedValuesMap]);

    const verifiedExtractedValues = useMemo(() => {
        return Object.values(verifiedExtractedValuesMap);
    }, [verifiedExtractedValuesMap]);

    return {
        logsheet,
        unverifiedExtractedValuesMap,
        unverifiedExtractedValues,
        verifiedExtractedValues,
        isLogsheetLoading,
    };
};
