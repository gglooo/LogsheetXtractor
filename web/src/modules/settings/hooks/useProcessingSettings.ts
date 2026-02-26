import { useState } from "react";

const PROCESSING_SETTINGS_STORAGE_KEY = "processing-settings";

type ProcessingSettings = {
    uglyCheckboxes: boolean;
};

export const useProcessingSettings = () => {
    const [processingSettings, setProcessingSettingsState] =
        useState<ProcessingSettings>(() => {
            const storedSettings = localStorage.getItem(
                PROCESSING_SETTINGS_STORAGE_KEY,
            );

            return storedSettings
                ? JSON.parse(storedSettings)
                : { uglyCheckboxes: false };
        });

    const setProcessingSettings = (newSettings: ProcessingSettings) => {
        setProcessingSettingsState(newSettings);
        localStorage.setItem(
            PROCESSING_SETTINGS_STORAGE_KEY,
            JSON.stringify(newSettings),
        );
    };

    return { processingSettings, setProcessingSettings };
};
