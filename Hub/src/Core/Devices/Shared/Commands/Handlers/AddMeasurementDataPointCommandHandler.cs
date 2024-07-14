using Core.Devices.ControlDevices;
using Core.Devices.Shared.Commands;
using Core.Sensors;
using Core.Shared.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Devices.Shared.Commands.Handlers
{
    public sealed class AddMeasurementDataPointCommandHandler : ICommandHandler<AddMeasurementDataPointCommand, NoResult>
    {
        private readonly ISensorRepository _sensorRepository;
        private readonly IControlDeviceRepository _controlDeviceRepository;

        public AddMeasurementDataPointCommandHandler(ISensorRepository sensorRepository, IControlDeviceRepository controlDeviceRepository)
        {
            _sensorRepository = sensorRepository;
            _controlDeviceRepository = controlDeviceRepository;
        }

        public async Task<NoResult> Handle(AddMeasurementDataPointCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var sensor = await _sensorRepository.GetByDeviceId(command.DeviceId);
                sensor.AddMeasurementDataPoint(command.Value, command.DataSource);
                await _sensorRepository.UpdateSensor(sensor);

                return new NoResult();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            try
            {
                var device = await _controlDeviceRepository.GetByDeviceId(command.DeviceId);
                device.AddMeasurementDataPoint(command.Value, command.DataSource);
                await _controlDeviceRepository.UpdateDevice(device);

                return new NoResult();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            throw new Exception($"No device exists: {command.DeviceId}");
        }
    }
}
