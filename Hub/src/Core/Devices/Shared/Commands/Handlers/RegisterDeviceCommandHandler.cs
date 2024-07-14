using Core.Devices.ControlDevices;
using Core.Devices.ControlDevices.WaterPump;
using Core.Devices.Shared.Commands;
using Core.Sensors;
using Core.Sensors.SoilMoisture;
using Core.Sensors.Temperature;
using Core.Shared.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Core.Devices.Shared.Commands.Handlers
{
    public sealed class RegisterDeviceCommandHandler : ICommandHandler<RegisterDeviceCommand, Device>
    {
        private readonly ISensorRepository _sensorRepository;
        private readonly IControlDeviceRepository _controlDeviceRepository;

        public RegisterDeviceCommandHandler(ISensorRepository sensorRepository, IControlDeviceRepository controlDeviceRepository)
        {
            _sensorRepository = sensorRepository;
            _controlDeviceRepository = controlDeviceRepository;
        }

        public async Task<Device> Handle(RegisterDeviceCommand command, CancellationToken cancellationToken)
        {
            var sensors = await _sensorRepository.GetAll();
            var devices = await _controlDeviceRepository.GetAll();
            var combinedDevices = new List<Device>();
            combinedDevices.AddRange(sensors);
            combinedDevices.AddRange(devices);

            if (combinedDevices.Any(d => d.UniqueId == command.UniqueId))
            {
                return combinedDevices.Single(d => d.UniqueId == command.UniqueId);
            }

            var newDeviceId = GenerateDeviceId(combinedDevices, command.UniqueId);

            switch (command.DeviceType)
            {
                case Device.DeviceType.TemperatureSensor:
                    var sensor = new TemperatureSensor(newDeviceId, command.UniqueId, "New Temperature Sensor " + newDeviceId);
                    return await _sensorRepository.AddSensor(sensor);
                case Device.DeviceType.SoilMoistureSensor:
                    var soilMoistureSensor = new SoilMoistureSensor(newDeviceId, command.UniqueId, "New Soil Moisture Sensor " + newDeviceId);
                    return await _sensorRepository.AddSensor(soilMoistureSensor);
                case Device.DeviceType.SoilTemperatureSensor:
                    break;
                case Device.DeviceType.WaterPumpDevice:
                    var device = new WaterPumpDevice(newDeviceId, command.UniqueId, "New Water pump " + newDeviceId);
                    return await _controlDeviceRepository.AddDevice(device);
            }

            throw new Exception($"Device seems not to have been created: {command}");
        }

        private byte GenerateDeviceId(List<Device> devices, uint uniqueId)
        {
            byte deviceId = (byte)devices.Count();
            var idAssigned = false;
            var loops = 0;
            byte maxId = 127;

            if (deviceId > maxId)
            {
                deviceId = 0;
            }

            while (loops < maxId)
            {
                if (!devices.Any(d => d.DeviceId == deviceId))
                {
                    idAssigned = true;
                    break;
                }

                deviceId++;

                if (deviceId > 127)
                {
                    deviceId = 0;
                }

                loops++;
            }

            if (!idAssigned)
            {
                throw new Exception($"No room for new Device! UniqueId: {uniqueId}");
            }

            return deviceId;
        }
    }
}
