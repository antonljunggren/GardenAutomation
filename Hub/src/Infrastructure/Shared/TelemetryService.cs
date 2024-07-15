using Core.Devices;
using Core.Shared;
using Core.Telemetry;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Devices.Device;

namespace Infrastructure.Shared
{
    internal sealed class TelemetryService : ITelemetryService
    {
        private class TelemetryMessage<T>
        {
            public string SystemId { get; set; }
            public T Details { get; set; }
            public string Type { get; }
            public DeviceType DeviceType { get; }
            public string DeviceName { get; }
            public uint DeviceUniqueId { get; }


            public TelemetryMessage(T details, string systemId, DeviceType deviceType, string deviceName, uint deviceUniqueId)
            {
                Details = details;
                SystemId = systemId;

                Type = typeof(T).Name;
                DeviceType = deviceType;
                DeviceName = deviceName;
                DeviceUniqueId = deviceUniqueId;
            }
        }

        private class DeviceStateTel
        {
            public string State { get; set; }

            public DeviceStateTel(string state)
            {
                State = state;
            }
        }

        private readonly ISystemRepository _systemRepository;
        private readonly string _apiUrl = "https://prod2-10.swedencentral.logic.azure.com:443/workflows/f31bd75647f6413db64f6f11552a9232/triggers/When_a_HTTP_request_is_received/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2FWhen_a_HTTP_request_is_received%2Frun&sv=1.0&sig=6RjSvxmY6WDqh1pthgJJ8pExP47x_romukFz2corIqY";

        public TelemetryService(ISystemRepository systemRepository)
        {
            _systemRepository = systemRepository;
        }

        public async Task SendDeviceState(string state, Device device)
        {
            try
            {
                var settings = await _systemRepository.GetSettingsAsync();

                if (string.IsNullOrWhiteSpace(settings.SystemId))
                {
                    return;
                }

                var stateObj = new DeviceStateTel(state);

                var msg = new TelemetryMessage<DeviceStateTel>(stateObj, settings.SystemId, device.Type, device.DeviceName, device.UniqueId);
                var msgJson = JsonConvert.SerializeObject(msg);

                using (var httpClient = new HttpClient())
                {
                    var content = new StringContent(msgJson, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await httpClient.PostAsync(_apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        Console.WriteLine("Error sending device state : " + response.StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending device state : {ex}");
            }

        }

        public async Task SendMeasuredData(MeasuredDataPoint dataPoint, Device device)
        {
            try
            {
                var settings = await _systemRepository.GetSettingsAsync();

                if (string.IsNullOrWhiteSpace(settings.SystemId))
                {
                    return;
                }

                var msg = new TelemetryMessage<MeasuredDataPoint>(dataPoint, settings.SystemId, device.Type, device.DeviceName, device.UniqueId);
                var msgJson = JsonConvert.SerializeObject(msg);

                using (var httpClient = new HttpClient())
                {
                    var content = new StringContent(msgJson, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await httpClient.PostAsync(_apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        Console.WriteLine("Error sending data point: " + response.StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending data point : {ex}");
            }
        }
    }
}
