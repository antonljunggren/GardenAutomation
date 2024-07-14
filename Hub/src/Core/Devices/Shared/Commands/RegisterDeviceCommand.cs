using Core.Shared.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Devices.Device;

namespace Core.Devices.Shared.Commands
{
    public sealed record RegisterDeviceCommand(uint UniqueId, DeviceType DeviceType) : ICommand<Device>
    {
    }
}
