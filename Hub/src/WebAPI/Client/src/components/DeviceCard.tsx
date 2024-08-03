import { Component, createSignal, Match, onMount, Show, Switch } from "solid-js";
import { Device, ControlDeviceState, DeviceType } from "../models/Device";

import unknownIcon from '../assets/unknown-device-icon.svg'
import temperatureSensorIcon from '../assets/temp-icon.svg'
import soilMoistureIcon from '../assets/soil-moisture-sensor-icon.svg'
import waterTapIcon from '../assets/water-tap-icon.svg'
import waterEmptyIcon from '../assets/water-level-icon-empty.svg'

import refreshIcon from '../assets/refresh-icon.svg'
import editIcon from '../assets/edit-icon.svg'

type DeviceCardProps = {
    device: Device;
    t: any;
    requestPumpScheduledState: (deviceId: number, minutes: number) => void;
    requestPumpState: (deviceId: number, turnOn: boolean) => void;
    onEdit: (device: Device) => void;
    refreshData: (device: Device) => void;
}

export const deviceIcon = (device: Device) => {
    switch (device.type) {
        case DeviceType.TemperatureSensor:
            return temperatureSensorIcon;
        case DeviceType.SoilMoistureSensor:
            return soilMoistureIcon;
        case DeviceType.WaterPumpDevice:
            return waterTapIcon;
        default:
            return unknownIcon;
    }
};

const DeviceCard: Component<DeviceCardProps> = (props) => {

    const device = props.device;

    const [waterLevelIcons, setWaterLevelIcons] = createSignal<string[]>([]);
    const [openPumpActions, setOpenPumpActions] = createSignal(false);

    onMount(async () => {
        const waterLevelIconsImport = [];
        for (let i = 10; i <= 100; i += 10) {
            waterLevelIconsImport.push(import(`../assets/water-level-icon-${i}.svg`));
        }

        const levelIconsModules = await Promise.all(waterLevelIconsImport);
        setWaterLevelIcons(levelIconsModules.map(m => m.default));
    });

    const waterLevelIcon = (device: Device) => {

        if (device.lastMeasuredDataPoints.length > 0) {
            let level = device.lastMeasuredDataPoints[0].value;

            if (level == 0.0) {
                return waterEmptyIcon;
            }

            let rounded = Math.round(level);
            let roundedToNearest10 = Math.ceil(rounded / 10) * 10;

            return waterLevelIcons()[(roundedToNearest10 / 10) - 1];
        }

        return waterEmptyIcon;
    }

    const getStatus = () => {
        switch (device.state) {
            case ControlDeviceState.Idle:
                return "idle";
            case ControlDeviceState.WaitingForResponse:
                return "waiting";
            case ControlDeviceState.PrimaryActionRunning:
                return "pumping";
        }
    }

    return (
        <div class="min-w-48 min-h-16 md:h-48 p-3">
            <div class="flex justify-between">
                <div class=" flex justify-start">
                    <img class=" w-12 mr-6" src={deviceIcon(device)} alt="N/A" />
                    <p class=" text-3xl my-auto">{device.deviceName}</p>
                </div>
                <div>
                    <button onclick={() => props.refreshData(props.device)}>
                        <img class=" w-6 mr-6" src={refreshIcon} alt="Refresh" />
                    </button>
                    <button onclick={() => props.onEdit(props.device)}>
                        <img class=" w-6 mr-6" src={editIcon} alt="Edit" />
                    </button>
                </div>
            </div>
            <div>
                <Switch>
                    <Match when={device.type === DeviceType.TemperatureSensor}>
                        <p class=" text-2xl">{device.lastMeasuredDataPoints.length > 0 ? device.lastMeasuredDataPoints[0].value.toString() : "N/A"}°C</p>
                    </Match>
                    <Match when={device.type === DeviceType.SoilMoistureSensor}>
                        <p class=" text-2xl">{device.lastMeasuredDataPoints.length > 0 ? device.lastMeasuredDataPoints[0].value.toFixed(1).toString() : "N/A"}%</p>
                    </Match>
                    <Match when={device.type === DeviceType.WaterPumpDevice}>
                        <div class="flex justify-between">
                            <div class="flex">
                                <img class=" w-24 mr-6" src={waterLevelIcon(device)} alt="N/A" />
                                <p class="my-auto text-2xl">{device.lastMeasuredDataPoints.length > 0 ? device.lastMeasuredDataPoints[0].value.toFixed(0).toString() : "N/A"}%</p>
                            </div>
                            <div class="block sm:flex gap-4">
                                <p class="my-auto text-2xl">
                                    {props.t("status")}: {props.t(getStatus())} {device.actionDurationStopTime && (`${props.t("until")} ${new Date(device.actionDurationStopTime).toLocaleTimeString()}`)}
                                </p>

                                <Show when={device.state === ControlDeviceState.PrimaryActionRunning}>
                                    <button class=" w-32 h-16 text-2xl bg-blue-500 hover:bg-blue-700 text-white font-bold my-4 py-2 px-4 rounded"
                                        onclick={() => props.requestPumpState(device.deviceId, false)}>Stop</button>
                                </Show>

                                <Show when={device.state === ControlDeviceState.Idle}>
                                    <div class="flex relative">
                                        <button class="w-32 h-16 text-2xl bg-blue-500 hover:bg-blue-700 text-white font-bold my-4 py-2 px-4 rounded-l"
                                            onclick={() => props.requestPumpState(device.deviceId, true)}>Start</button>

                                        <button class="w-8 text-white h-16 my-4 py-2 bg-blue-500 hover:bg-blue-700 rounded-r"
                                            type="button"
                                            onClick={() => setOpenPumpActions(!openPumpActions())}>
                                            <svg class="w-2.5 h-2.5 ms-3" aria-hidden="true" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 10 6">
                                                <path stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="m1 1 4 4 4-4" />
                                            </svg>
                                        </button>

                                        {openPumpActions() && (
                                            <div class="w-40 absolute left-0 top-20 border bg-white border-blue-700">
                                                <ul class="py-2 text-md text-gray-700 dark:text-gray-200" aria-labelledby="dropdownDefaultButton">
                                                    <li>
                                                        <button class="px-4 py-2 hover:bg-gray-100"
                                                            onclick={() => props.requestPumpScheduledState(device.deviceId, 10)}>
                                                            {props.t("runTask10Min")}
                                                        </button>
                                                    </li>
                                                    <li>
                                                        <button class="px-4 py-2 hover:bg-gray-100"
                                                            onclick={() => props.requestPumpScheduledState(device.deviceId, 30)}>
                                                            {props.t("runTask30Min")}
                                                        </button>
                                                    </li>
                                                    <li>
                                                        <button class="px-4 py-2 hover:bg-gray-100"
                                                            onclick={() => props.requestPumpScheduledState(device.deviceId, 60)}>
                                                            {props.t("runTask60Min")}
                                                        </button>
                                                    </li>
                                                </ul>
                                            </div>
                                        )}
                                    </div>

                                </Show>
                            </div>
                        </div>
                    </Match>
                </Switch>
            </div>
        </div>
    );
};

export default DeviceCard;