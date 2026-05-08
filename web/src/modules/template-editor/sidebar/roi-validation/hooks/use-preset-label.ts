import { defineMessages, useIntl } from "react-intl";

const presetLabelMessages = defineMessages({
    year: {
        id: "template-editor.roi-validation.predefined.code.year",
        defaultMessage: "Year",
    },
    month: {
        id: "template-editor.roi-validation.predefined.code.month",
        defaultMessage: "Month",
    },
    day: {
        id: "template-editor.roi-validation.predefined.code.day",
        defaultMessage: "Day",
    },
    latitude: {
        id: "template-editor.roi-validation.predefined.code.latitude",
        defaultMessage: "Latitude",
    },
    latitudeHemisphere: {
        id: "template-editor.roi-validation.predefined.code.latitudeHemisphere",
        defaultMessage: "Latitude Hemisphere (N/S)",
    },
    latitudeDegrees: {
        id: "template-editor.roi-validation.predefined.code.latitudeDegrees",
        defaultMessage: "Latitude Degrees (DD)",
    },
    latitudeMinutes: {
        id: "template-editor.roi-validation.predefined.code.latitudeMinutes",
        defaultMessage: "Latitude Minutes (MM.MMM)",
    },
    longitude: {
        id: "template-editor.roi-validation.predefined.code.longitude",
        defaultMessage: "Longitude",
    },
    longitudeHemisphere: {
        id: "template-editor.roi-validation.predefined.code.longitudeHemisphere",
        defaultMessage: "Longitude Hemisphere (E/W)",
    },
    longitudeDegrees: {
        id: "template-editor.roi-validation.predefined.code.longitudeDegrees",
        defaultMessage: "Longitude Degrees (DDD)",
    },
    longitudeMinutes: {
        id: "template-editor.roi-validation.predefined.code.longitudeMinutes",
        defaultMessage: "Longitude Minutes (MM.MMM)",
    },
    hours: {
        id: "template-editor.roi-validation.predefined.code.hours",
        defaultMessage: "Hours",
    },
    minutes: {
        id: "template-editor.roi-validation.predefined.code.minutes",
        defaultMessage: "Minutes",
    },
    waterTemperatureC: {
        id: "template-editor.roi-validation.predefined.code.waterTemperatureC",
        defaultMessage: "Water Temperature C",
    },
    ph: {
        id: "template-editor.roi-validation.predefined.code.ph",
        defaultMessage: "pH",
    },
    dissolvedOxygenMgL: {
        id: "template-editor.roi-validation.predefined.code.dissolvedOxygenMgL",
        defaultMessage: "Dissolved Oxygen mg/L",
    },
    windSpeedMs: {
        id: "template-editor.roi-validation.predefined.code.windSpeedMs",
        defaultMessage: "Wind Speed m/s",
    },
    windDirectionDeg: {
        id: "template-editor.roi-validation.predefined.code.windDirectionDeg",
        defaultMessage: "Wind Direction deg",
    },
});

type PresetCode = keyof typeof presetLabelMessages;

const hasPresetMessage = (code: string): code is PresetCode =>
    code in presetLabelMessages;

export const usePresetLabel = () => {
    const intl = useIntl();

    return (code: string, fallback: string) => {
        if (!hasPresetMessage(code)) {
            return fallback;
        }

        return intl.formatMessage(presetLabelMessages[code]);
    };
};
