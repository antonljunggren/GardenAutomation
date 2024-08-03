using Core.Shared.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Shared
{
    public sealed record ScheduledCommand(DateTime StopTime, Type CommandType, object[] Args);

    public class SystemSettings : ICloneable
    {
        public string SystemId { get; set; } = "";
        public bool ConnectToCloud { get; set; } = false;
        public List<ScheduledCommand> ScheduledCommands { get; set; } = new List<ScheduledCommand>();

        public object Clone()
        {
            return new SystemSettings
            {
                SystemId = SystemId,
                ConnectToCloud = ConnectToCloud,
                ScheduledCommands = ScheduledCommands
            };
        }
    }
}
