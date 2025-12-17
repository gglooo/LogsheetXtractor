import type { ReactNode } from "react";
import { IntlProvider } from "react-intl";

import enMessages from "@/i18n/messages.json";

const messages = {
    en: enMessages,
};

interface I18nProviderProps {
    children: ReactNode;
}

export function I18nProvider({ children }: I18nProviderProps) {
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
