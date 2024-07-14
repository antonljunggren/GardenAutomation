import { Component } from "solid-js";
import type * as i18n from "@solid-primitives/i18n";
import { useLocalization } from "../LocalizationProvider";

import ukFlag from '../assets/uk_flag.png'
import sweFlag from '../assets/swe_flag.png'

import homeIcon from '../assets/home-icon.svg'
import settingsIcon from '../assets/settings-icon.svg'
import langIcon from '../assets/lang-icon.svg'


const Navbar: Component = () => {

    const { locale, changeLocale, t } = useLocalization();

    const flagOpacity = (lang: string) => {
        let currentLang = localStorage.getItem("language") ?? "en";

        if (lang === currentLang) {
            return "100%";
        }

        return "40%"
    }

    const toggleLanguageMenu = () => {
        const languageMenu = document.getElementById('language-menu');
        languageMenu?.classList.toggle('hidden');
    };

    return (
        <>
            {/* Desktop navbar */}
            <div class="hidden md:block">
                <nav class="bg-white fixed w-full z-20 top-0 start-0 border-b border-gray-200">
                    <div class="max-w-screen-xl flex flex-wrap items-center justify-between mx-auto p-4">
                        <div class="items-center justify-between w-full md:flex md:w-auto md:order-1" id="navbar-sticky">
                            <ul class="flex flex-col p-4 md:p-0 mt-4 font-medium border border-gray-100 rounded-lg bg-gray-50 md:space-x-8 rtl:space-x-reverse md:flex-row md:mt-0 md:border-0 md:bg-white">
                                <li>
                                    <a class=" text-2xl" href="/">{t("home")}</a>
                                </li>
                                <li>
                                    <a class=" text-2xl" href="/settings">{t("settings")}</a>
                                </li>
                            </ul>
                        </div>
                        <div class="items-center justify-between w-full md:flex md:w-auto md:order-2">
                            <button class="w-10 mr-1" style={"opacity:" + flagOpacity("en")} onClick={() => changeLocale("en")}>
                                <img class="object-cover" src={ukFlag} alt={t("english")} />
                            </button>
                            <button class="w-10" style={"opacity:" + flagOpacity("sv")} onClick={() => changeLocale("sv")}>
                                <img class="object-cover" src={sweFlag} alt={t("swedish")} />
                            </button>
                        </div>
                    </div>
                </nav>

                <div class="my-20"></div>
            </div>

            {/* Mobile navbar */}
            <div class="md:hidden">
                <div class="my-4"></div>
                <nav class="bg-white fixed bottom-0 w-full z-20 border-t border-gray-200">
                    <div class="max-w-screen-xl flex justify-around items-center mx-auto p-2">
                        <a class="flex flex-col items-center" href="/">
                            <img class="w-6 h-6" src={homeIcon} alt={t("home")} />
                            <span class="text-xs">{t("home")}</span>
                        </a>

                        <a class="flex flex-col items-center" href="/settings">
                            <img class="w-6 h-6" src={settingsIcon} alt={t("settings")} />
                            <span class="text-xs">{t("settings")}</span>
                        </a>

                        <div class="relative">
                            <button id="language-button" class="flex flex-col items-center" onClick={() => toggleLanguageMenu()}>
                                <img class="w-6 h-6" src={langIcon} alt={t("language")} />
                                <span class="text-xs">{t("language")}</span>
                            </button>
                            <div id="language-menu" class="absolute bottom-12 left-1/2 transform -translate-x-1/2 bg-white border rounded-lg shadow-lg hidden">
                                <button class="w-10 m-2" style={"opacity:" + flagOpacity("en")} onClick={() => { changeLocale("en"); toggleLanguageMenu(); }}>
                                    <img class="object-cover" src={ukFlag} alt={t("english")} />
                                </button>
                                <button class="w-10 m-2" style={"opacity:" + flagOpacity("sv")} onClick={() => { changeLocale("sv"); toggleLanguageMenu(); }}>
                                    <img class="object-cover" src={sweFlag} alt={t("swedish")} />
                                </button>
                            </div>
                        </div>
                    </div>
                </nav>
            </div>
        </>
    );
};

export default Navbar;