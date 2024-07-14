using Core.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Shared
{
    internal sealed class SystemRepository : ISystemRepository
    {
        private readonly string _systemSettingsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "system_settings.json");
        private SystemSettings _systemSettings;

        public SystemRepository()
        {
            if (File.Exists(_systemSettingsFile))
            {
                var jsonData = File.ReadAllText(_systemSettingsFile);
                var settings = JsonConvert.DeserializeObject<SystemSettings>(jsonData);

                if (settings is null)
                {
                    _systemSettings = new SystemSettings();
                    Task.Run(WriteToFile);
                }
                else
                {
                    _systemSettings = settings;
                }
            }
            else
            {
                _systemSettings = new SystemSettings();
                Task.Run(WriteToFile);
            }
        }

        private async Task WriteToFile()
        {
            var jsonData = JsonConvert.SerializeObject(_systemSettings);
            var dir = Path.GetDirectoryName(_systemSettingsFile);
            if (dir is not null && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            await File.WriteAllTextAsync(_systemSettingsFile, jsonData);

            Debug.WriteLine("Written to settings file");
        }

        public Task<SystemSettings> GetSettingsAsync()
        {
            throw new NotImplementedException();
        }
    }
}
