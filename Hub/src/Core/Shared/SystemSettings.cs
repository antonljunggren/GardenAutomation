using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Shared
{
    public class SystemSettings : ICloneable
    {
        public string SystemId { get; set; } = "";
        public bool ConnectToCloud { get; set; } = true;

        public object Clone()
        {
            return new SystemSettings
            {
                SystemId = SystemId,
                ConnectToCloud = ConnectToCloud,
            };
        }
    }
}
