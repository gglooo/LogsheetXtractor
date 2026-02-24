import { createContext } from "react";

export type Language = "en" | "cs" | "de";

export type LanguageContextType = {
    locale: Language;
    setLocale: (locale: Language) => void;
};

export const LanguageContext = createContext<LanguageContextType | undefined>(
    undefined,
);
