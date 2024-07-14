export enum DeviceType {
    TemperatureSensor = 0,
    SoilMoistureSensor = 1,
    SoilTemperatureSensor = 2,
    WaterPumpDevice = 3,
}

export enum SensorDeviceState {
    Normal = 0,
    Error = 1,
    Unresponsive = 2,
}

export enum ControlDeviceState {
    Idle = 0,
    WaitingForResponse = 1,
    PrimaryActionRunning = 2,
}

export enum DataPointType {
    Temperature = 0,
    SoilMoisture = 1,
    Humidity,
    WaterLevel
}

export type MeasuredDataPoint = {
    type: DataPointType;
    value: number;
    dateTime: Date;
}

export type Device = {
    type: DeviceType;
    deviceId: number;
    deviceName: string;
    state: number;
    lastMeasuredDataPoints: MeasuredDataPoint[];
}