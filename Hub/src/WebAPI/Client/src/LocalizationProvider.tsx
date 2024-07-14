import { createSignal, createMemo, createContext, useContext } from "solid-js";
import * as i18n from "@solid-primitives/i18n";
import * as en from "./i18n/en";
import * as sv from "./i18n/sv";

const dictionaries = {
    en: en.dict,
    sv: sv.dict,
};

type Locale = "en" | "sv";

interface LocalizationContextValue {
    locale: Locale;
    changeLocale: (locale: Locale) => void;
    t: any; // Adjust the type based on your translator function's return type
}

const LocalizationContext = createContext<LocalizationContextValue>();

export const LocalizationProvider = (props: any) => {
    const lco = localStorage.getItem("language") ?? "en";
    const [locale, setLocale] = createSignal<Locale>(lco as Locale);

    const changeLocale = (locale: Locale) => {
        localStorage.setItem("language", locale);
        setLocale(locale);
    }

    const dict = createMemo(() => i18n.flatten(dictionaries[locale()]));

    dict();

    const t = i18n.translator(dict, i18n.resolveTemplate);

    return (
        <LocalizationContext.Provider value={{ locale: locale(), changeLocale, t }}>
            {props.children}
        </LocalizationContext.Provider>
    );
}

export const useLocalization = () => useContext(LocalizationContext)!;