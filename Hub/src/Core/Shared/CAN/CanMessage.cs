namespace Core.Shared.CAN
{
    public enum CanMessageType
    {
        RegisterRequest = 0x0,
        RegisterResponse = 0x1,
        Data = 0x2, //to get primary data value (temp, moisture ...)
        Ping = 0x3,
        Command = 0x4, //to set state and get data values if more than one exists
        DeviceState = 0x5,
    }

    public sealed class CanMessage
    {
        private const uint MESSAGE_TYPE_MASK = 0b00000011110000000000000000000000;
        private const uint DEVICE_ID_MASK = 0b00000000001111111100000000000000;

        public CanMessageType MessageType { get; private set; }
        public byte DeviceID { get; private set; }
        public byte[] Data { get; private set; }

        public CanMessage(uint canId, byte[] data)
        {
            //id bits order, (3) priority, (4) msg type, (8) deviceId 7 bits actual, rest reserved
            int messageType = Convert.ToInt32((canId & MESSAGE_TYPE_MASK) >> 22);
            DeviceID = (byte)((canId & DEVICE_ID_MASK) >> 14);
            if (Enum.IsDefined(typeof(CanMessageType), messageType))
            {
                MessageType = (CanMessageType) messageType;
            }
            else
            {
                throw new Exception($"Wrong CAN Id for message type, canId:[{canId}]");
            }

            Data = data;
        }

        public CanMessage(CanMessageType type, byte deviceId, byte[] data)
        {
            MessageType = type;
            Data = data;
            DeviceID = deviceId;
        }

        public override string? ToString()
        {
            string dataAsHex = string.Join(" ", Data.Select((x) => x.ToString("X2")));
            return $"CAN Msg: [{MessageType}], device: [{DeviceID}], data: [{dataAsHex}]";
        }
    }
}
