using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Infrastructure.BackgroundServices
{
    internal sealed class SystemTelemetryService : BackgroundService
    {
        private record SystemSettings
        {
            public string SystemId { get; set; } = GenerateSystemId();

            private static string GenerateSystemId()
            {
                var guid = Guid.NewGuid();
                byte[] guidBytes = guid.ToByteArray();
                string base64String = Convert.ToBase64String(guidBytes);

                string shortId = base64String
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');

                return shortId;
            }
        }

        private readonly string _systemSettingsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "system_settings.json");
        private SystemSettings _systemSettings;

        public SystemTelemetryService()
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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
