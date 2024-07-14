using Iot.Device.SocketCan;
using Core.Shared.CAN;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Infrastructure.CAN
{
    internal sealed class CanService : ICanService
    {
        public event EventHandler<CanMessage>? CanMessageReceived = default!;
        private CanRaw _canBus = new CanRaw();
        private bool _loopStarted = false;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public void Start()
        {
            Task.Run(() => ReceiveLoop(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
        }

        public void Stop()
        {
            Debug.WriteLine("Stopping receive loop for CAN controller");
            _cancellationTokenSource.Cancel();
            _loopStarted = false;
        }

        private void ReceiveLoop(CancellationToken cancellationToken)
        {
            if(_loopStarted)
            {
                return;
            }

            Debug.WriteLine("Starting receive loop for CAN controller");

            _loopStarted = true;

            byte[] buffer = new byte[8];

            while(!cancellationToken.IsCancellationRequested)
            {
                bool validFrame = _canBus.TryReadFrame(buffer, out int frameLength, out CanId id);
                Span<byte> data = new Span<byte>(buffer, 0, frameLength);

                if (!validFrame)
                {
                    Debug.WriteLine($"Invalid frame received!");
                    string type = id.ExtendedFrameFormat ? "EFF" : "SFF";
                    string dataAsHex = string.Join(" ", data.ToArray().Select((x) => x.ToString("X2")));
                    Debug.WriteLine($"Id: 0x{id.Value:X2} [{type}]: {dataAsHex}");
                    continue;
                }
                try
                {
                    CanMessage canMessage = new CanMessage(id.Value, data.ToArray());

                    CanMessageReceived?.Invoke(null, canMessage);
                } 
                catch(Exception ex)
                {
                    Debug.WriteLine(ex);
                    throw;
                }
                
            }

            Debug.WriteLine("Ending receive loop for CAN controller");
        }

        public void SendCanMessage(CanMessage canMessage)
        {
            try
            {
                uint combinedId = (0b000 << 26) //highest priority
                           | ((uint)canMessage.MessageType << 22)
                           | ((uint)canMessage.DeviceID << 14);

                CanId id = new CanId()
                {
                    Extended = combinedId
                };
                
                string dataAsHex = string.Join(" ", canMessage.Data.Select((x) => x.ToString("X2")));
                Debug.WriteLine($"Send message: id-data: {combinedId} - [{dataAsHex}]");

                _canBus.WriteFrame(canMessage.Data, id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"error sending can message: {ex}");
                throw;
            }
        }
    }
}
