using Core.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Sensors
{
    public interface ISensorRepository
    {
        Task<List<SensorDevice>> GetAll();
        Task<bool> SensorExists(byte devideId);
        Task<SensorDevice> GetByDeviceId(byte deviceId);
        Task<SensorDevice> GetByUniqueId(uint uniqueId);
        Task<SensorDevice> UpdateSensor(SensorDevice sensor);
        Task<SensorDevice> AddSensor(SensorDevice sensor);
    }
}
