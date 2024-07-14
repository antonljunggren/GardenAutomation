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
                
                if(settings is null)
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
            //ping hello to cloud
            using (HttpClient client = new HttpClient())
            {
                // Create the content to send
                var content = new StringContent($"{{\"id\": \"{_systemSettings.SystemId}\"}}", Encoding.UTF8, "application/json");

                // Send the POST request
                HttpResponseMessage response = await client.PostAsync("https://prod2-30.swedencentral.logic.azure.com:443/workflows/16b684ed651c49ba8c7a7d68464579c6/triggers/When_a_HTTP_request_is_received/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2FWhen_a_HTTP_request_is_received%2Frun&sv=1.0&sig=dqAXri6qabaOFJAIiS1ThY3J1tPu3QqfvjSAE5-j63g", content);

                if (response.IsSuccessStatusCode)
                {
                    Debug.WriteLine("Message sent successfully!");
                }
                else
                {
                    Debug.WriteLine($"Failed to send message. Status code: {response.StatusCode}");
                }
            }

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
