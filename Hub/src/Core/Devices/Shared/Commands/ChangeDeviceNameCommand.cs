using Core.Shared.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Devices.Shared.Commands
{
    public sealed record ChangeDeviceNameCommand(byte deviceId, string newName) : ICommand
    {
    }
}
