using Core.Shared.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Devices.MeasuredDataPoint;

namespace Core.Devices.Shared.Commands
{
    public sealed record AddMeasurementDataPointCommand(byte DeviceId, int Value, byte DataSource) : ICommand
    {
    }
}
