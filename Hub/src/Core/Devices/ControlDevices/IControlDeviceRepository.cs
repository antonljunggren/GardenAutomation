using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Devices.ControlDevices
{
    public interface IControlDeviceRepository
    {
        Task<List<ControlDevice>> GetAll();
        Task<ControlDevice> GetByDeviceId(byte deviceId);
        Task<ControlDevice> GetByUniqueId(uint uniqueId);
        Task<ControlDevice> UpdateDevice(ControlDevice device);
        Task<ControlDevice> AddDevice(ControlDevice device);
    }
}
