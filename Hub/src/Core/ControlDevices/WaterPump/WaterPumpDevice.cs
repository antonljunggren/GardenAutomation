using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.ControlDevices;
using Core.Devices;
using static Core.Devices.MeasuredDataPoint;

namespace Core.ControlDevices.WaterPump
{
    public sealed class WaterPumpDevice : ControlDevice
    {
        private enum DataSources
        {
            WaterLevel = 0,
        }

        public enum CommandTypes
        {
            ToggleWaterPump = 0,
            CalibrateLowLevel = 1,
            CalibrateMaxLevel = 2
        }

        public WaterPumpDevice(byte deviceId, uint uniqueId, string deviceName) : base(DeviceType.WaterPumpDevice, deviceId, uniqueId, deviceName)
        {
        }

        public void SetIsPumping(bool isPumping)
        {
            if (isPumping)
            {
                SetState(ControlDeviceState.PrimaryActionRunning);
            }
            else
            {
                SetState(ControlDeviceState.Idle);
            }

        }

        protected override MeasuredDataPoint HandleDataSource(int value, byte dataSource)
        {
            DataPointType dataPointType;
            if (dataSource == (byte)DataSources.WaterLevel)
            {
                dataPointType = DataPointType.WaterLevel;

                float decimalValue = value / 1023f * 100.0f;

                return new MeasuredDataPoint(dataPointType, decimalValue, DateTime.UtcNow);
            }

            throw new NotImplementedException($"Water pump does not handle data type: {dataSource}");
        }

        public override List<byte> GetDataSourceTypes()
        {
            return Enum.GetValues(typeof(DataSources)).Cast<DataSources>().Select(s => (byte)s).ToList();
        }
    }
}
