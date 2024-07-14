using Core.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Devices.MeasuredDataPoint;

namespace Core.Sensors.SoilMoisture
{
    public sealed class SoilMoistureSensor : SensorDevice
    {
        public SoilMoistureSensor(byte deviceId, uint uniqueId, string deviceName) : base(DeviceType.SoilMoistureSensor, deviceId, uniqueId, deviceName)
        {
        }

        private enum DataSources
        {
            Primary = 0
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

                const int minInput = 240;
                const int maxInput = 675;

                float input = Math.Clamp(value, minInput, maxInput);

                float decimalValue = 100.0f - ((input-minInput) / (maxInput-minInput)) * 100.0f;

                return new MeasuredDataPoint(dataPointType, decimalValue, DateTime.UtcNow);
            }

            throw new NotImplementedException($"Soil moisture sensor does not handle data source: {dataSource}");
        }
    }
}
