using Core.Devices.ControlDevices;
using Core.Devices.Shared.Commands;
using Core.Sensors;
using Core.Shared.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Devices.Shared.Commands.Handlers
{
    public sealed class ChangeDeviceNameCommandHandler : ICommandHandler<ChangeDeviceNameCommand, NoResult>
    {
        private readonly ISensorRepository _sensorRepository;
        private readonly IControlDeviceRepository _controlDeviceRepository;

        public ChangeDeviceNameCommandHandler(ISensorRepository sensorRepository, IControlDeviceRepository controlDeviceRepository)
        {
            _sensorRepository = sensorRepository;
            _controlDeviceRepository = controlDeviceRepository;
        }

        public async Task<NoResult> Handle(ChangeDeviceNameCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var sensor = await _sensorRepository.GetByDeviceId(command.deviceId);
                sensor.SetDeviceName(command.newName);
                await _sensorRepository.UpdateSensor(sensor);

                return new NoResult();
            } 
            catch (Exception)
            {

            }

            try
            {
                var device = await _controlDeviceRepository.GetByDeviceId(command.deviceId);
                device.SetDeviceName(command.newName);
                await _controlDeviceRepository.UpdateDevice(device);

                return new NoResult();
            }
            catch (Exception)
            {

            }

            throw new Exception($"No device exists: {command.deviceId}");
        }
    }
}
