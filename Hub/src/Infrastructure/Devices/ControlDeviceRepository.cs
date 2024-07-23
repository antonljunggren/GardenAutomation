using Core.Devices;
using Core.ControlDevices.WaterPump;
using Core.Sensors;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.ControlDevices.ControlDevice;
using System.Diagnostics;
using Core.ControlDevices;

namespace Infrastructure.Devices
{
    internal sealed class ControlDeviceRepository : IControlDeviceRepository
    {
        private List<ControlDevice> _devices = new List<ControlDevice>();
        private readonly string _dataFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "control-devices.json");
        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public ControlDeviceRepository()
        {
            if (File.Exists(_dataFilePath))
            {
                var jsonData = File.ReadAllText(_dataFilePath);
                var devicesJson = JsonConvert.DeserializeObject<JObject[]>(jsonData);

                if (devicesJson is not null)
                {
                    foreach (var device in devicesJson)
                    {
                        try
                        {
                            var deviceType = Enum.Parse<Device.DeviceType>(device.GetValue(nameof(ControlDevice.Type))!.ToString());
                            var deviceId = device.GetValue(nameof(Device.DeviceId))!.Value<byte>();
                            var uniqueId = device.GetValue(nameof(Device.UniqueId))!.Value<uint>();
                            var deviceName = device.GetValue(nameof(Device.DeviceName))!.ToString();

                            if (deviceType == Device.DeviceType.WaterPumpDevice)
                            {
                                var waterPump = new WaterPumpDevice(deviceId, uniqueId, deviceName);
                                _devices.Add(waterPump);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e);
                        }
                    }
                }

                Debug.WriteLine($"Loaded devices found {_devices?.Count ?? 0}");
            }
        }

        private async Task WriteToFile()
        {
            var jsonData = JsonConvert.SerializeObject(_devices);
            await _semaphoreSlim.WaitAsync();
            Debug.WriteLine("Prepare write to file");

            var dir = Path.GetDirectoryName(_dataFilePath);
            if (dir is not null && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            await File.WriteAllTextAsync(_dataFilePath, jsonData);
            Debug.WriteLine("Written to file");
            _semaphoreSlim.Release();
        }

        private List<ControlDevice> GetAsReadOnly()
        {
            return new ReadOnlyCollection<ControlDevice>(_devices).ToList();
        }

        public async Task<ControlDevice> AddDevice(ControlDevice device)
        {
            if (_devices.Contains(device))
            {
                throw new Exception($"Device already exists, id:{device.DeviceId}");
            }

            _devices.Add(device);
            await WriteToFile();
            return await GetByDeviceId(device.DeviceId);
        }

        public Task<List<ControlDevice>> GetAll()
        {
            return Task.FromResult(GetAsReadOnly());
        }

        public Task<ControlDevice> GetByDeviceId(byte deviceId)
        {
            return Task.FromResult(GetAsReadOnly().Single(d => d.DeviceId == deviceId));
        }

        public Task<ControlDevice> GetByUniqueId(uint uniqueId)
        {
            return Task.FromResult(GetAsReadOnly().Single(d => d.UniqueId == uniqueId));
        }

        public async Task<ControlDevice> UpdateDevice(ControlDevice device)
        {
            var index = _devices.IndexOf(_devices.Single(d => d.UniqueId == device.UniqueId));
            _devices.RemoveAt(index);
            _devices.Insert(index, device);
            await WriteToFile();
            return device;
        }

        public Task<bool> DeviceExists(byte devideId)
        {
            return Task.FromResult(_devices.Any(d => d.DeviceId == devideId));
        }
    }
}
