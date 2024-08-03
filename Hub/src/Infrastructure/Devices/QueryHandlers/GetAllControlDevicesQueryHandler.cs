using Core.ControlDevices;
using Core.ControlDevices.Queries;
using Core.Shared.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Devices.QueryHandlers
{
    internal sealed class GetAllControlDevicesQueryHandler : IQueryHandler<GetAllControlDevicesQuery, List<ControlDevice>>
    {
        private readonly IControlDeviceRepository _controlDeviceRepository;
        public GetAllControlDevicesQueryHandler(IControlDeviceRepository controlDeviceRepository)
        {
            _controlDeviceRepository = controlDeviceRepository;
        }

        public async Task<List<ControlDevice>> Handle(GetAllControlDevicesQuery query, CancellationToken cancellationToken)
        {
            var devices = await _controlDeviceRepository.GetAll();
            return devices;
        }
    }
}
