using Core.Shared.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ControlDevices.WaterPump.Commands
{
    public sealed record ToggleWaterPumpCommand(byte DeviceId, bool TurnOn) : ICommand
    {
    }
}
