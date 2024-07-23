using Core.ControlDevices;
using Core.Devices;
using Core.Devices.Shared.Queries;
using Core.Sensors;
using Core.Shared.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Devices.QueryHandlers
{
    internal sealed class GetAllDevicesQueryHandler : IQueryHandler<GetAllDevicesQuery, List<Device>>
    {
        private readonly ISensorRepository _sensorRepository;
        private readonly IControlDeviceRepository _controlDeviceRepository;

        public GetAllDevicesQueryHandler(ISensorRepository sensorRepository, IControlDeviceRepository controlDeviceRepository)
        {
            _sensorRepository = sensorRepository;
            _controlDeviceRepository = controlDeviceRepository;
        }

        public async Task<List<Device>> Handle(GetAllDevicesQuery query, CancellationToken cancellationToken)
        {
            var sensors = new Queue<SensorDevice>(await _sensorRepository.GetAll());
            var controlDevices = new Queue<ControlDevice>(await _controlDeviceRepository.GetAll());
            var result = new List<Device>();

            while (sensors.Count > 0 || controlDevices.Count > 0)
            {
                if (sensors.Count >= 2)
                {
                    result.Add(sensors.Dequeue());
                    result.Add(sensors.Dequeue());
                }
                else if (sensors.Count > 0)
                {
                    result.Add(sensors.Dequeue());
                }

                if (controlDevices.Count > 0)
                {
                    result.Add(controlDevices.Dequeue());
                }
            }

            return result;
        }
    }
}
