using static Core.Devices.MeasuredDataPoint;

namespace Core.Devices
{

    public abstract class Device
    {
        public enum DeviceType
        {
            TemperatureSensor = 0x0,
            SoilMoistureSensor = 0x1,
            SoilTemperatureSensor = 0x2,
            WaterPumpDevice = 0x3,
        }

        public DeviceType Type { get; private set; }
        public abstract uint State { get; }
        public byte DeviceId { get; private set; }
        public uint UniqueId { get; private set; }
        public string DeviceName { get; private set; }
        private Dictionary<DataPointType, MeasuredDataPoint> _lastMeasuredDataPoints = new Dictionary<DataPointType, MeasuredDataPoint>();
        public IReadOnlyList<MeasuredDataPoint> LastMeasuredDataPoints => _lastMeasuredDataPoints.Values.ToList().AsReadOnly();

        public Device(DeviceType type, byte deviceId, uint uniqueId, string deviceName)
        {
            Type = type;
            DeviceId = deviceId;
            UniqueId = uniqueId;
            DeviceName = deviceName;
        }

        public void SetDeviceName(string newName)
        {
            if (string.IsNullOrEmpty(newName))
            {
                throw new Exception("Name is empty");
            }

            DeviceName = newName;
        }

        protected abstract MeasuredDataPoint HandleDataSource(int value, byte dataSource);

        public void AddMeasurementDataPoint(int value, byte dataSource)
        {
            var dataPoint = HandleDataSource(value, dataSource);
            if (_lastMeasuredDataPoints.ContainsKey(dataPoint.Type))
            {
                _lastMeasuredDataPoints[dataPoint.Type] = dataPoint;
            }
            else
            {
                _lastMeasuredDataPoints.Add(dataPoint.Type, dataPoint);
            }
        }

        public abstract List<byte> GetDataSourceTypes();
    }
}
