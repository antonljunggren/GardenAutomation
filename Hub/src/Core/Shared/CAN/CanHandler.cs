using Core.Devices.ControlDevices.WaterPump;
using Core.Devices.ControlDevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Devices.Shared.Commands;
using Core.Devices;
using static Core.Devices.Device;
using Core.Shared.Commands;
using System.Diagnostics;

namespace Core.Shared.CAN
{
    public abstract class CanHandler
    {
        private readonly ICanService _canService;
        private readonly IControlDeviceRepository _controlDeviceRepository;
        private readonly ICommandDispatcher _commandDispatcher;

        protected CanHandler(ICanService canService, IControlDeviceRepository controlDeviceRepository, ICommandDispatcher commandDispatcher)
        {
            _canService = canService;
            _controlDeviceRepository = controlDeviceRepository;
            canService.CanMessageReceived += CanMessageReceived;
            _commandDispatcher = commandDispatcher;
        }

        protected abstract void CanMessageReceived(object? sender, CanMessage msg);

        protected async Task RegisterDevice(CanMessage msg)
        {
            try
            {
                byte[] paddedArr = new byte[4];
                for (int i = 0; i < msg.Data.Length-1; i++)
                {
                    paddedArr[i] = msg.Data[i];
                }
                var uniqueId = BitConverter.ToUInt32(paddedArr);
                var devieType = msg.Data.Last();

                var cmd = new RegisterDeviceCommand(uniqueId, (DeviceType)devieType);
                Device registeredDevice = await _commandDispatcher.Dispatch<RegisterDeviceCommand, Device>(cmd, CancellationToken.None);

                byte[] respArr = new byte[5];
                for (int i = 0; i < msg.Data.Length-1; i++)
                {
                    respArr[i] = msg.Data[i];
                }
                respArr[4] = registeredDevice.DeviceId;
                CanMessage response = new CanMessage(CanMessageType.RegisterResponse, 0, respArr);
                _canService.SendCanMessage(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }

            
        }

        protected async Task HandleDeviceState(CanMessage msg)
        {
            try
            {
                var device = await _controlDeviceRepository.GetByDeviceId(msg.DeviceID);

                var state = msg.Data[0];

                if (device.Type == Core.Devices.Device.DeviceType.WaterPumpDevice)
                {
                    var togglePump = false;
                    if (state == 1)
                    {
                        togglePump = true;
                    }

                    var pump = device as WaterPumpDevice;

                    if (pump is null)
                    {
                        throw new Exception("Cannot cast to Water Pump");
                    }

                    pump.SetIsPumping(togglePump);

                    await _controlDeviceRepository.UpdateDevice(pump);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        protected async Task HandleMeasurementData(CanMessage msg)
        {
            try
            {
                byte[] value = new byte[4];
                Array.Copy(msg.Data, value, 2);
                int val = BitConverter.ToInt32(value);

                var cmd = new AddMeasurementDataPointCommand(msg.DeviceID, val, 0);
                await _commandDispatcher.Dispatch<AddMeasurementDataPointCommand, NoResult>(cmd, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}
