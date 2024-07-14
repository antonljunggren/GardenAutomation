namespace Core.Devices
{
    public sealed class MeasuredDataPoint
    {
        public enum DataPointType
        {
            Temperature = 0,
            SoilMoisture,
            Humidity,
            WaterLevel
        }

        public DataPointType Type { get; }
        public float Value { get; }
        public DateTime DateTime { get; }

        public MeasuredDataPoint(DataPointType type, float value, DateTime dateTime)
        {
            Type = type;
            Value = value;
            DateTime = dateTime;
        }
    }
}
