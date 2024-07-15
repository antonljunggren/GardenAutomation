using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Shared
{
    public interface ISystemRepository
    {
        Task<SystemSettings> GetSettingsAsync();
        Task UpdateSettings(SystemSettings newSettings);
    }
}
