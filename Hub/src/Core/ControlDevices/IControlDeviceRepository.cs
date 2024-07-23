using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ControlDevices
{
    public interface IControlDeviceRepository
    {
        Task<List<ControlDevice>> GetAll();
        Task<bool> DeviceExists(byte devideId);
        Task<ControlDevice> GetByDeviceId(byte deviceId);
        Task<ControlDevice> GetByUniqueId(uint uniqueId);
        Task<ControlDevice> UpdateDevice(ControlDevice device);
        Task<ControlDevice> AddDevice(ControlDevice device);
    }
}
