import * as i18n from "@solid-primitives/i18n";

export const dict = {
    english: "English",
    swedish: "Swedish",
    home: "Home",
    settings: "Settings",
    dashboard: "Dashboard",
    pumping: "pumping",
    status: "Status",
    idle: "ready",
    waiting: "waiting for a response",
    language: "Language",
    close: "close",
    editDevice: "Edit device",
    deviceName: "device name",
    save: "save",
    calibrateLowLevel: "calibrate low level",
    calibrateMaxLevel: "calibrate max level",
    errorMessage: "something went wrong",
    dataRefreshedMessage: "device data was refreshed",
    deviceUpdateMessage: "device was updated",
    runTask10Min: "Run for 10 minutes",
    runTask30Min: "Run for 30 minutes",
    runTask60Min: "Run for 60 minutes",
    until: "until",

    hello: i18n.template<{ name: string }>("hello {{ name }}, how are you?"),
    goodbye: ({ name }: { name: string }) => `goodbye ${name}`,
    food: {
        meat: "meat",
    },
};