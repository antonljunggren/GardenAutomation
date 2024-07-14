using Core.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Sensors
{
    public abstract class SensorDevice : Device
    {
        public enum SensorDeviceState
        {
            Normal = 0,
            Error = 1,
            Unresponsive = 2,
        }

        public SensorDeviceState SensorState { get; private set; }
        public override uint State => (uint)SensorState;

        protected SensorDevice(DeviceType type, byte deviceId, uint uniqueId, string deviceName) : base(type, deviceId, uniqueId, deviceName)
        {
            SensorState = SensorDeviceState.Normal;
        }
    }
}
