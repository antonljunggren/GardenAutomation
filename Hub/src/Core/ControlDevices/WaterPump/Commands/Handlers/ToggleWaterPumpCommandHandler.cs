using Core.ControlDevices;
using Core.ControlDevices.WaterPump;
using Core.ControlDevices.WaterPump.Commands;
using Core.Devices;
using Core.Shared.CAN;
using Core.Shared.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ControlDevices.WaterPump.Commands.Handlers
{
    public sealed class ToggleWaterPumpCommandHandler : ICommandHandler<ToggleWaterPumpCommand, NoResult>
    {
        private readonly IControlDeviceRepository _controlDeviceRepository;
        private readonly ICanService _canService;

        public ToggleWaterPumpCommandHandler(IControlDeviceRepository controlDeviceRepository, ICanService canService)
        {
            _controlDeviceRepository = controlDeviceRepository;
            _canService = canService;
        }

        public async Task<NoResult> Handle(ToggleWaterPumpCommand command, CancellationToken cancellationToken)
        {
            var device = await _controlDeviceRepository.GetByDeviceId(command.DeviceId);

            if (device.Type != Device.DeviceType.WaterPumpDevice)
            {
                throw new Exception($"Wrong device type for command: {device.Type}");
            }

            var pump = device as WaterPumpDevice;

            if (pump is null)
            {
                throw new Exception("Cannot cast to Water Pump");
            }

            var turnOn = command.TurnOn ? 1 : 0;
            var toggleCanMsg = new CanMessage(CanMessageType.Command, pump.DeviceId, new byte[] { (byte)WaterPumpDevice.CommandTypes.ToggleWaterPump, (byte)turnOn });
            _canService.SendCanMessage(toggleCanMsg);

            pump.SetIsWaitingForResponse();
            await _controlDeviceRepository.UpdateDevice(pump);

            //delay to hopefully get updated state when client fetches data after this response
            await Task.Delay(200);

            return new NoResult();
        }
    }
}
