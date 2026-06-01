import { useState } from "react";

const USER_SETTINGS_STORAGE_KEY = "user-settings";
const LEGACY_PROCESSING_SETTINGS_STORAGE_KEY = "processing-settings";

type UserSettings = {
    uglyCheckboxes: boolean;
    automaticAlignmentOnUpload: boolean;
};

const defaultUserSettings: UserSettings = {
    uglyCheckboxes: false,
    automaticAlignmentOnUpload: true,
};

export const useUserSettings = () => {
    const [userSettings, setUserSettingsState] = useState<UserSettings>(() => {
        const storedSettings =
            localStorage.getItem(USER_SETTINGS_STORAGE_KEY) ??
            localStorage.getItem(LEGACY_PROCESSING_SETTINGS_STORAGE_KEY);

        if (!storedSettings) {
            return defaultUserSettings;
        }

        try {
            const parsed = JSON.parse(storedSettings) as Partial<UserSettings>;
            return {
                uglyCheckboxes:
                    parsed.uglyCheckboxes ??
                    defaultUserSettings.uglyCheckboxes,
                automaticAlignmentOnUpload:
                    parsed.automaticAlignmentOnUpload ??
                    defaultUserSettings.automaticAlignmentOnUpload,
            };
        } catch {
            return defaultUserSettings;
        }
    });

    const setUserSettings = (newSettings: UserSettings) => {
        setUserSettingsState(newSettings);
        localStorage.setItem(
            USER_SETTINGS_STORAGE_KEY,
            JSON.stringify(newSettings),
        );
    };

    return { userSettings, setUserSettings };
};
