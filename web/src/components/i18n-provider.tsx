import type { ReactNode } from "react";
import { IntlProvider } from "react-intl";

import enMessages from "@/i18n/messages.json";

// In a real app, you'd load these from a file or API per locale
const messages = {
    en: enMessages,
    // 'cs': { ... }
};

interface I18nProviderProps {
    children: ReactNode;
}

export function I18nProvider({ children }: I18nProviderProps) {
    // Hardcoded to English for now, can be expanded to use state/context
    const locale = "en";

    return (
        <IntlProvider
            locale={locale}
            defaultLocale="en"
            messages={messages[locale]}
        >
            {children}
        </IntlProvider>
    );
}
