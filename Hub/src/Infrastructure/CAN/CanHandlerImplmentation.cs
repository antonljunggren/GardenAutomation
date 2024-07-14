using Core.Devices.ControlDevices;
using Core.Devices.ControlDevices.WaterPump;
using Core.Shared.CAN;
using Core.Shared.Commands;
using Infrastructure.Devices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;

namespace Infrastructure.CAN
{
    internal sealed class CanHandlerImplmentation : CanHandler
    {
        public CanHandlerImplmentation(ICanService canService, IControlDeviceRepository controlDeviceRepository, ICommandDispatcher commandDispatcher) : base(canService, controlDeviceRepository, commandDispatcher)
        {
        }

        protected override void CanMessageReceived(object? sender, CanMessage msg)
        {
            Debug.WriteLine($"Received: {msg}");

            if (msg.MessageType == CanMessageType.RegisterRequest)
            {
                Task.Run(() => RegisterDevice(msg));
            }

            if (msg.MessageType == CanMessageType.DeviceState)
            {
                Task.Run(() => HandleDeviceState(msg));
            }

            if(msg.MessageType == CanMessageType.Data)
            {
                Task.Run(() => HandleMeasurementData(msg));
            }
        }
    }
}
