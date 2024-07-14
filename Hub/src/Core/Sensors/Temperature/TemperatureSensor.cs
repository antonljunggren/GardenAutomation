using Core.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Devices.MeasuredDataPoint;

namespace Core.Sensors.Temperature
{
    public sealed class TemperatureSensor : SensorDevice
    {
        private enum DataSources
        {
            Primary = 0
        }

        public TemperatureSensor(byte deviceId, uint uniqueId, string deviceName) : base(DeviceType.TemperatureSensor, deviceId, uniqueId, deviceName)
        {
        }

        public override List<byte> GetDataSourceTypes()
        {
            return Enum.GetValues(typeof(DataSources)).Cast<DataSources>().Select(s => (byte)s).ToList();
        }

        protected override MeasuredDataPoint HandleDataSource(int value, byte dataSource)
        {
            DataPointType dataPointType;
            if (dataSource == (byte)DataSources.Primary)
            {
                dataPointType = DataPointType.Temperature;

                float decimalValue = ((float)value) / 100f;

                return new MeasuredDataPoint(dataPointType, decimalValue, DateTime.UtcNow);
            }

            throw new NotImplementedException($"Water pump does not handle data source: {dataSource}");
        }
    }
}
