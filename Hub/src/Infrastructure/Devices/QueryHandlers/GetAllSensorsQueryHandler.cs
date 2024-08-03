using Core.Sensors;
using Core.Sensors.Queries;
using Core.Shared.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Devices.QueryHandlers
{
    internal sealed class GetAllSensorsQueryHandler : IQueryHandler<GetAllSensorsQuery, List<SensorDevice>>
    {
        private readonly ISensorRepository _sensorRepository;

        public GetAllSensorsQueryHandler(ISensorRepository sensorRepository)
        {
            _sensorRepository = sensorRepository;
        }

        public async Task<List<SensorDevice>> Handle(GetAllSensorsQuery query, CancellationToken cancellationToken)
        {
            var sensors = await _sensorRepository.GetAll();
            return sensors;
        }
    }
}
