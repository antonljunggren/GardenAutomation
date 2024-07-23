using Core.Devices;
using Core.ControlDevices.WaterPump;
using Core.Shared.CAN;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Devices.Device;

namespace Infrastructure.CAN
{
    internal sealed class SimulatedCanService : ICanService
    {
        public event EventHandler<CanMessage>? CanMessageReceived;

        private class SimCanDevice()
        {
            public byte DeviceId;
            public uint UniqueId;
            public DeviceType DeviceType;
            public uint State;

            public static SimCanDevice Create(byte deviceId, uint uniqueId, DeviceType deviceType, uint state)
            {
                return new SimCanDevice()
                {
                    DeviceId = deviceId,
                    UniqueId = uniqueId,
                    DeviceType = deviceType,
                    State = state
                };
            }
        }

        private List<SimCanDevice> _devices = new List<SimCanDevice>();

        public SimulatedCanService()
        {
            _ = Task.Run(InitDevices);
        }

        private async Task InitDevices()
        {
            await Task.Delay(1000);
            Debug.WriteLine("Registering devices from simualted CAN bus");
            var temperatureDevice1 = SimCanDevice.Create(0, 100, DeviceType.TemperatureSensor, 0);
            _devices.Add(temperatureDevice1);
            SendRegisterCommand(temperatureDevice1);
            await Task.Delay(500);

            var soilMoistDevice1 = SimCanDevice.Create(0, 150, DeviceType.SoilMoistureSensor, 0);
            _devices.Add(soilMoistDevice1);
            SendRegisterCommand(soilMoistDevice1);
            await Task.Delay(500);

            var waterPump1 = SimCanDevice.Create(0, 200, DeviceType.WaterPumpDevice, 0);
            _devices.Add(waterPump1);
            SendRegisterCommand(waterPump1);
        }

        private void SendRegisterCommand(SimCanDevice device)
        {
            var data = new byte[5];
            var uniqueIdArr = BitConverter.GetBytes(device.UniqueId);


            for (int i = 0; i < 4; i++)
            {
                data[i] = uniqueIdArr[i];
            }

            data[4] = (byte)device.DeviceType;

            var regcmd = new CanMessage(CanMessageType.RegisterRequest, 0, data);

            CanMessageReceived?.Invoke(null, regcmd);
        }

        //Simulate the hub sending messages on the bus
        //So simulate the devices handling the data and responding
        public void SendCanMessage(CanMessage canMessage)
        {
            Debug.WriteLine($"Send simulated CAN msg: {canMessage}");

            switch (canMessage.MessageType)
            {
                case CanMessageType.RegisterResponse:

                    var uniqueId = BitConverter.ToUInt32(canMessage.Data.Take(4).ToArray());

                    var regDev = _devices.Single(d => d.UniqueId == uniqueId);
                    regDev.DeviceId = canMessage.Data.Last();

                    Debug.WriteLine($"device {regDev.UniqueId} got deviceId {regDev.DeviceId}");
                    break;

                case CanMessageType.Data:
                    var dataDev = _devices.Single(d => d.DeviceId == canMessage.DeviceID);
                    if (dataDev.DeviceType == DeviceType.TemperatureSensor)
                    {
                        var temperatureMsg = new CanMessage(CanMessageType.Data, dataDev.DeviceId, BitConverter.GetBytes(2653));
                        CanMessageReceived?.Invoke(null, temperatureMsg);
                    }

                    if (dataDev.DeviceType == DeviceType.SoilMoistureSensor)
                    {
                        var soilMoistureMsg = new CanMessage(CanMessageType.Data, dataDev.DeviceId, BitConverter.GetBytes(512));
                        CanMessageReceived?.Invoke(null, soilMoistureMsg);
                    }

                    if (dataDev.DeviceType == DeviceType.WaterPumpDevice)
                    {
                        var waterLevelMsg = new CanMessage(CanMessageType.Data, dataDev.DeviceId, BitConverter.GetBytes(512));
                        CanMessageReceived?.Invoke(null, waterLevelMsg);
                    }
                    break;

                case CanMessageType.Command:
                    var cmdDev = _devices.Single(d => d.DeviceId == canMessage.DeviceID);
                    var cmdType = (WaterPumpDevice.CommandTypes)canMessage.Data[0];
                    var cmdVal = canMessage.Data[1];

                    if (cmdDev.DeviceType == DeviceType.WaterPumpDevice)
                    {
                        if (cmdType == WaterPumpDevice.CommandTypes.ToggleWaterPump)
                        {
                            cmdDev.State = cmdVal;
                            var waterPumpStateMsg = new CanMessage(CanMessageType.DeviceState, cmdDev.DeviceId, [cmdVal]);
                            CanMessageReceived?.Invoke(null, waterPumpStateMsg);
                        }
                    }

                    break;
            }
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }
    }
}
