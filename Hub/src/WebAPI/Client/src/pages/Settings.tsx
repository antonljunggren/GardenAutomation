import { Component } from "solid-js";
import { useLocalization } from "../LocalizationProvider";

const Settings: Component = () => {

    const { locale, changeLocale, t } = useLocalization();

    return (
        <>
            <h1 class=" text-2xl mb-4">{t("settings")}</h1>
        </>
    );
};
export default Settings;