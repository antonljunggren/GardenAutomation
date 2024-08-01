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
        private readonly string _systemSettingsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "GardenAutomationData", "system_settings.json");
        private SystemSettings _systemSettings;
        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public SystemRepository()
        {
            var oldFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "system_settings.json");

            //TODO: Remove some time after release, wen deployed devices have hopefully updated

            if (File.Exists(oldFilePath))
            {
                var dir = Path.GetDirectoryName(_systemSettingsFile);
                if (dir is not null && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.Move(oldFilePath, _systemSettingsFile);

                if (File.Exists(oldFilePath))
                {
                    File.Delete(oldFilePath);
                }
            }

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

            await _semaphoreSlim.WaitAsync();

            if (dir is not null && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            await File.WriteAllTextAsync(_systemSettingsFile, jsonData);

            _semaphoreSlim.Release();
            Debug.WriteLine("Written to settings file");
        }

        public Task<SystemSettings> GetSettingsAsync()
        {
            var res = _systemSettings.Clone() as SystemSettings;
            return Task.FromResult(res!);
        }

        public async Task UpdateSettings(SystemSettings newSettings)
        {
            _systemSettings.ConnectToCloud = newSettings.ConnectToCloud;
            _systemSettings.SystemId = newSettings.SystemId;

            await WriteToFile();
        }
    }
}
