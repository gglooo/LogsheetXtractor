import { createContext } from "react";

export type Language = "en" | "cs" | "de" | "fr" | "es" | "it" | "pt";

export const supportedLanguages: { code: Language; label: string }[] = [
    { code: "en", label: "English" },
    { code: "cs", label: "Čeština" },
    { code: "de", label: "Deutsch" },
    { code: "fr", label: "Français" },
    { code: "es", label: "Español" },
    { code: "it", label: "Italiano" },
    { code: "pt", label: "Português" },
];

export type LanguageContextType = {
    locale: Language;
    setLocale: (locale: Language) => void;
};

export const LanguageContext = createContext<LanguageContextType | undefined>(
    undefined,
);
