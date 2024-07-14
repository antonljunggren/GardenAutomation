using Core.Shared.CAN;
using Core.Shared.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Devices.ControlDevices.WaterPump.Commands.Handlers
{
    public sealed class CalibrateWaterLevelCommandHandler : ICommandHandler<CalibrateWaterLevelCommand, NoResult>
    {
        private readonly ICanService _canService;
        private readonly IControlDeviceRepository _controlDeviceRepository;

        public CalibrateWaterLevelCommandHandler(ICanService canService, IControlDeviceRepository controlDeviceRepository)
        {
            _canService = canService;
            _controlDeviceRepository = controlDeviceRepository;
        }

        public async Task<NoResult> Handle(CalibrateWaterLevelCommand command, CancellationToken cancellationToken)
        {
            var device = await _controlDeviceRepository.GetByDeviceId(command.DeviceId);

            byte cmdType = (byte)CalibrateWaterLevelCommand.WaterLevelCalibrationType.LowLevel;

            if(command.levelType == CalibrateWaterLevelCommand.WaterLevelCalibrationType.MaxLevel)
            {
                cmdType = (byte)CalibrateWaterLevelCommand.WaterLevelCalibrationType.MaxLevel;
            }

            var canMsg = new CanMessage(CanMessageType.Command, command.DeviceId, [cmdType, (byte)0]);

            _canService.SendCanMessage(canMsg);

            return new NoResult();
        }
    }
}
