using Core.Shared.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Devices.ControlDevices.WaterPump.Commands.CalibrateWaterLevelCommand;

namespace Core.Devices.ControlDevices.WaterPump.Commands
{
    public sealed record CalibrateWaterLevelCommand(byte DeviceId, WaterLevelCalibrationType levelType) : ICommand
    {
        public enum WaterLevelCalibrationType
        {
            LowLevel = 1,
            MaxLevel = 2
        }
    }
}
