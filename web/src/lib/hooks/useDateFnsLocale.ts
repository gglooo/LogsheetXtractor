import { cs, de, enUS, es, fr, it, pt } from "date-fns/locale";
import { useIntl } from "react-intl";

export const useDateFnsLocale = () => {
    const intl = useIntl();

    switch (intl.locale) {
        case "cs":
            return cs;
        case "en":
            return enUS;
        case "de":
            return de;
        case "es":
            return es;
        case "fr":
            return fr;
        case "it":
            return it;
        case "pt":
            return pt;
        default:
            return enUS;
    }
};
