import { Component, For, Match, Switch, createResource, createSignal, onCleanup } from "solid-js";
import { Device, DeviceType } from "../models/Device";
import { useLocalization } from "../LocalizationProvider";
import Loading from "../components/Loading";
import DeviceCard, { deviceIcon } from "../components/DeviceCard";
import DeviceEditCard from "../components/DeviceEditCard";
import { useNotification } from "../NotificationProvider";



const Home: Component = () => {

    const hostname = import.meta.env.VITE_HOST === undefined ? new URL(window.location.href).hostname : import.meta.env.VITE_HOST;
    const port = import.meta.env.VITE_PORT === undefined ? 8080 : import.meta.env.VITE_PORT;

    let initialLoad = false;
    const fetchDevices = async () => {
        let devices = (await fetch(`http://${hostname}:${port}/api/device`, {
            mode: 'cors',
            headers: {
                'Access-Control-Allow-Origin': hostname
            }
        })).json();
        initialLoad = true;
        return devices;
    }

    const { createNotification } = useNotification();
    const { locale, changeLocale, t } = useLocalization();
    const [devices, { mutate, refetch }] = createResource(fetchDevices);

    const [isEditDialogOpen, setEditDialogOpen] = createSignal(false);
    const [editDevice, setEditDevice] = createSignal<Device | null>(null);

    const requestPumpState = async (deviceId: number, turnOn: boolean) => {
        await fetch(`http://${hostname}:${port}/api/device/waterpump/${deviceId}/toggle/${turnOn}`, {
            method: 'POST',
            mode: 'cors',
            headers: {
                'Access-Control-Allow-Origin': hostname
            }
        }).then(() => {
            refetch();
            createNotification('success', t("deviceUpdateMessage"));
        }).catch(() => {
            createNotification('error', t("errorMessage"));
        });
    }

    const calibrateWaterLevel = async (deviceId: number, levelType: number) => {
        await fetch(`http://${hostname}:${port}/api/device/waterpump/${deviceId}/calibrate/${levelType}`, {
            method: 'POST',
            mode: 'cors',
            headers: {
                'Access-Control-Allow-Origin': hostname
            }
        }).then(() => {
            refetch();
            createNotification('success', t("deviceUpdateMessage"));
        }).catch(() => {
            createNotification('error', t("errorMessage"));
        });
    }

    const refreshDeviceData = async (deviceId: number) => {
        await fetch(`http://${hostname}:${port}/api/device/${deviceId}/data/0`, {
            method: 'POST',
            mode: 'cors',
            headers: {
                'Access-Control-Allow-Origin': hostname
            }
        }).then(() => {
            refetch();
            createNotification('success', t("dataRefreshedMessage"));
        }).catch(() => {
            createNotification('error', t("errorMessage"));
        });
    }

    const changeName = async (deviceId: number, newName: string) => {
        await fetch(`http://${hostname}:${port}/api/device/${deviceId}/name/${newName}`, {
            method: 'POST',
            mode: 'cors',
            headers: {
                'Access-Control-Allow-Origin': hostname
            }
        }).then(() => {
            refetch();
            createNotification('success', t("deviceUpdateMessage"));
        }).catch(() => {
            createNotification('error', t("errorMessage"));
        });
    }

    const timer = setInterval(() => {
        refetch();
    }, 15000);

    onCleanup(() => clearInterval(timer));

    return (
        <div>
            <h1 class=" text-2xl mb-4">{t("dashboard")}</h1>

            <Switch>
                <Match when={devices.loading && !initialLoad}>
                    <Loading />
                </Match>
                <Match when={devices.error}>
                    <p>Error</p>
                    <Loading />
                </Match>
                <Match when={devices()}>

                    <div class="grid grid-flow-row grid-cols-1 md:grid-cols-2 lg:grid-cols-4 2xl:grid-cols-6">
                        <For each={devices()}>
                            {(device: Device, index) =>
                                <div class={"border border-gray-200" + (device.type === DeviceType.WaterPumpDevice ? " md:row-span-2 md:col-span-2" : "")}>
                                    <DeviceCard device={device} t={t} requestPumpState={requestPumpState}
                                        onEdit={(device) => {
                                            setEditDevice(device);
                                            setEditDialogOpen(true);
                                        }}
                                        refreshData={(device) => refreshDeviceData(device.deviceId)} />
                                </div>
                            }
                        </For>
                    </div>
                    <DeviceEditCard open={isEditDialogOpen()}
                        onClose={() => setEditDialogOpen(false)}
                        t={t}
                        deviceName={editDevice()?.deviceName ?? ""}
                        deviceIcon={editDevice() != null ? deviceIcon(editDevice()!) : ""}
                        deviceType={editDevice()?.type ?? DeviceType.TemperatureSensor}
                        changeName={(newName: string) => changeName(editDevice()?.deviceId ?? 0, newName)}
                        calibrateWaterLevel={(leveltype: number) => calibrateWaterLevel(editDevice()?.deviceId ?? 0, leveltype)}
                    />
                </Match>
            </Switch>
        </div>
    );
};

export default Home;