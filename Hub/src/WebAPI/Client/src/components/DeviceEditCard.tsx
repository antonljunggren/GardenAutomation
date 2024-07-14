import { Component, Show, createEffect } from "solid-js";
import { Device, DeviceType } from "../models/Device";

type DeviceEditProps = {
    open: boolean;
    deviceName: string;
    deviceIcon: string;
    deviceType: DeviceType
    t: any;
    changeName: (newName: string) => void;
    calibrateWaterLevel: (levelType: number) => void;
    onClose: () => void;
};

const DeviceEditCard: Component<DeviceEditProps> = (props) => {
    let dialogRef: HTMLDialogElement | undefined;
    let deviceName = props.deviceName;

    createEffect(() => {
        if (props.open) {
            dialogRef?.showModal();
        } else {
            dialogRef?.close();
        }
    });

    return (
        <dialog ref={dialogRef} class=" w-10/12 md:w-1/3 p-4">
            <div class=" flex justify-start">
                <img class="m-4 w-12 mr-10" src={props.deviceIcon} alt="N/A" />
                <h1 class="text-2xl my-auto">{props.t("editDevice")}</h1>
                <p></p>
            </div>
            <div class=" mb-8">
                <label for="deviceName">{props.t("deviceName")}:</label>
                <input class="m-2 border" id="deviceName"
                    name="deviceName"
                    type="text"
                    value={props.deviceName}
                    onchange={(d) => deviceName = d.target.value}
                    placeholder={props.t("deviceName")} />

                <button class=" p-2 bg-green-400 hover:bg-green-300 border rounded" onClick={() => {
                    if (deviceName.length > 0 && deviceName !== props.deviceName) {
                        props.changeName(deviceName);
                    }
                }}>{props.t("save")}</button>
            </div>

            <Show when={props.deviceType === DeviceType.WaterPumpDevice}>
                <div>
                    <button class=" p-2 bg-green-400 hover:bg-green-300 border rounded" onClick={() => {
                        props.calibrateWaterLevel(1);
                    }}>{props.t("calibrateLowLevel")}</button>

                    <button class=" p-2 bg-green-400 hover:bg-green-300 border rounded" onClick={() => {
                        props.calibrateWaterLevel(2);
                    }}>{props.t("calibrateMaxLevel")}</button>
                </div>
            </Show>

            <div class="flex justify-center md:justify-end gap-2">
                <button class=" p-2 bg-gray-200 hover:bg-gray-100 border rounded" onClick={() => {
                    deviceName = "";
                    props.onClose();
                }}>{props.t("close")}</button>
            </div>

        </dialog>
    );
};

export default DeviceEditCard;