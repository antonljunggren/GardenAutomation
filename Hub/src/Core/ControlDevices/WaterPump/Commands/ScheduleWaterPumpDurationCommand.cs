using Core.Shared.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ControlDevices.WaterPump.Commands
{
    public sealed record ScheduleWaterPumpDurationCommand(byte DeviceId, int Minutes) : ICommand
    {
    }
}
