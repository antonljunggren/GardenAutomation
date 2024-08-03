using Core.Devices;
using Core.Shared;
using Core.Shared.CAN;
using Core.Shared.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ControlDevices.WaterPump.Commands.Handlers
{
    public sealed class ScheduleWaterPumpDurationCommandHandler : ICommandHandler<ScheduleWaterPumpDurationCommand, NoResult>
    {
        private readonly IControlDeviceRepository _controlDeviceRepository;
        private readonly ICanService _canService;
        private readonly ISystemRepository _systemRepository;

        public ScheduleWaterPumpDurationCommandHandler(IControlDeviceRepository controlDeviceRepository, ICanService canService, ISystemRepository systemRepository)
        {
            _controlDeviceRepository = controlDeviceRepository;
            _canService = canService;
            _systemRepository = systemRepository;
        }

        public async Task<NoResult> Handle(ScheduleWaterPumpDurationCommand command, CancellationToken cancellationToken)
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

            var stopTime = DateTime.UtcNow + TimeSpan.FromMinutes(command.Minutes);

            var toggleCanMsg = new CanMessage(CanMessageType.Command, pump.DeviceId, new byte[] { (byte)WaterPumpDevice.CommandTypes.ToggleWaterPump, (byte)1 });
            _canService.SendCanMessage(toggleCanMsg);
            pump.SetIsWaitingForResponse();
            pump.SetActionDurationStopTime(stopTime);
            await _controlDeviceRepository.UpdateDevice(pump);

            
            var scheduledCommand = new ScheduledCommand(stopTime, typeof(ToggleWaterPumpCommand), new object[] { command.DeviceId, false });

            var system = await _systemRepository.GetSettingsAsync();
            system.ScheduledCommands.Add(scheduledCommand);
            await _systemRepository.UpdateSettings(system);

            await Task.Delay(200);

            return new NoResult();
        }
    }
}
