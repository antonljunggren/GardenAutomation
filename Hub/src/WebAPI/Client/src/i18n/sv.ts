import * as i18n from "@solid-primitives/i18n";

export const dict = {
    english: "Engelska",
    swedish: "Svenska",
    home: "Hem",
    settings: "Inställningar",
    dashboard: "Överblick",
    pumping: "pumpar",
    status: "Status",
    idle: "redo",
    waiting: "väntar på svar",
    language: "Språk",
    close: "stäng",
    editDevice: "Redigera enhet",
    deviceName: "namn på enhet",
    save: "spara",
    calibrateLowLevel: "kalibrera botten",
    calibrateMaxLevel: "kalibrera toppen",
    errorMessage: "något fick fel",
    dataRefreshedMessage: "enhetsdata har laddats om",
    deviceUpdateMessage: "enhet har uppdaterats",
    runTask10Min: "Kör i 10 minuter",
    runTask30Min: "Kör i 30 minuter",
    runTask60Min: "Kör i 60 minuter",


    hello: i18n.template<{ name: string }>("hej {{ name }}, hur mår du?"),
    goodbye: ({ name }: { name: string }) => `hejdå ${name}`,
    food: {
        meat: "kött",
    },
};