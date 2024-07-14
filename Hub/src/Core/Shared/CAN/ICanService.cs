using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Shared.CAN
{
    public interface ICanService
    {
        event EventHandler<CanMessage>? CanMessageReceived;
        void Start();
        void Stop();
        void SendCanMessage(CanMessage canMessage);
    }
}
