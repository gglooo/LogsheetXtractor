import { type ReactNode, useEffect, useState } from "react";
import { IntlProvider } from "react-intl";

import csMessages from "@/i18n/cs.json";
import deMessages from "@/i18n/de.json";
import enMessages from "@/i18n/en.json";
import { type Language, LanguageContext } from "./language-context";

const messages: Record<string, Record<string, string>> = {
    en: enMessages,
    cs: csMessages,
    de: deMessages,
};

type I18nProviderProps = {
    children: ReactNode;
};

export function I18nProvider({ children }: I18nProviderProps) {
    const [locale, setLocaleState] = useState<Language>(() => {
        const storedLocale = localStorage.getItem("app-locale") as Language;
        return storedLocale && messages[storedLocale] ? storedLocale : "en";
    });

    const setLocale = (newLocale: Language) => {
        setLocaleState(newLocale);
        localStorage.setItem("app-locale", newLocale);
    };

    useEffect(() => {
        document.documentElement.lang = locale;
    }, [locale]);

    return (
        <LanguageContext.Provider value={{ locale, setLocale }}>
            <IntlProvider
                locale={locale}
                defaultLocale="en"
                messages={messages[locale]}
            >
                {children}
            </IntlProvider>
        </LanguageContext.Provider>
    );
}
