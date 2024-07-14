using Core.Devices;
using Core.Sensors;
using Core.Sensors.SoilMoisture;
using Core.Sensors.Temperature;
using Iot.Device.FtCommon;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Devices
{
    internal sealed class SensorRepository : ISensorRepository
    {
        private List<SensorDevice> _sensors = new List<SensorDevice>();
        private readonly string _dataFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "sensors.json");
        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public SensorRepository()
        {
            if(File.Exists(_dataFilePath))
            {
                var jsonData = File.ReadAllText(_dataFilePath);
                var sensorsJson = JsonConvert.DeserializeObject<JObject[]>(jsonData);

                if(sensorsJson is not null)
                {
                    foreach(var sensor in sensorsJson)
                    {
                        try
                        {
                            var sensorType = Enum.Parse<Device.DeviceType>(sensor.GetValue(nameof(SensorDevice.Type))!.ToString());
                            var deviceId = sensor.GetValue(nameof(Device.DeviceId))!.Value<byte>();
                            var uniqueId = sensor.GetValue(nameof(Device.UniqueId))!.Value<uint>();
                            var deviceName = sensor.GetValue(nameof(Device.DeviceName))!.ToString();

                            if (sensorType == Device.DeviceType.TemperatureSensor)
                            {
                                var tempSensor = new TemperatureSensor(deviceId, uniqueId, deviceName);
                                _sensors.Add(tempSensor);
                            }

                            if(sensorType == Device.DeviceType.SoilMoistureSensor)
                            {
                                var soilMoistureSensor = new SoilMoistureSensor(deviceId, uniqueId, deviceName);
                                _sensors.Add(soilMoistureSensor);
                            }
                        }
                        catch(Exception e)
                        {
                            Debug.WriteLine(e);
                        }
                    }
                }

                Debug.WriteLine($"Loaded sensors found {_sensors?.Count ?? 0}");
            }
        }

        private async Task WriteToFile()
        {
            var jsonData = JsonConvert.SerializeObject(_sensors);
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

        private List<SensorDevice> GetAsReadOnly()
        {
            return new ReadOnlyCollection<SensorDevice>(_sensors).ToList();
        }

        public async Task<SensorDevice> AddSensor(SensorDevice sensor)
        {
            if (_sensors.Contains(sensor))
            {
                throw new Exception($"Sensor already exists, id:{sensor.DeviceId}");
            }

            _sensors.Add(sensor);
            await WriteToFile();
            return await GetByDeviceId(sensor.DeviceId);
        }

        public Task<List<SensorDevice>> GetAll()
        {
            return Task.FromResult(GetAsReadOnly());
        }

        public Task<SensorDevice> GetByDeviceId(byte deviceId)
        {
            return Task.FromResult(GetAsReadOnly().Single(s => s.DeviceId == deviceId));
        }

        public Task<SensorDevice> GetByUniqueId(uint uniqueId)
        {
            return Task.FromResult(GetAsReadOnly().Single(s => s.UniqueId == uniqueId));
        }

        public async Task<SensorDevice> UpdateSensor(SensorDevice sensor)
        {
            var index = _sensors.IndexOf(_sensors.Single(s => s.UniqueId == sensor.UniqueId));
            _sensors.RemoveAt(index);
            _sensors.Insert(index, sensor);
            await WriteToFile();
            return sensor;
        }
    }
}
