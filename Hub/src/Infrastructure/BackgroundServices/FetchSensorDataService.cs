using Core.ControlDevices;
using Core.Sensors;
using Core.Shared.CAN;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.BackgroundServices
{
    internal sealed class FetchSensorDataService : BackgroundService
    {
        public readonly ICanService _canService;
        public readonly ISensorRepository _sensorRepository;
        private readonly IControlDeviceRepository _controlDeviceRepository;

        public FetchSensorDataService(ICanService canService, ISensorRepository sensorRepository, IControlDeviceRepository controlDeviceRepository)
        {
            _canService = canService;
            _sensorRepository = sensorRepository;
            _controlDeviceRepository = controlDeviceRepository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(10000, stoppingToken);
            Debug.WriteLine("Starting background task to fetch sensor data");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var sensors = await _sensorRepository.GetAll();

                    foreach (var sensor in sensors)
                    {
                        var sources = sensor.GetDataSourceTypes();
                        foreach (var sourceType in sources)
                        {
                            var csnMsg = new CanMessage(CanMessageType.Data, sensor.DeviceId, [sourceType]);
                            _canService.SendCanMessage(csnMsg);
                            await Task.Delay(100);
                        }
                    }

                    var devices = await _controlDeviceRepository.GetAll();

                    foreach(var device in devices)
                    {
                        var sources = device.GetDataSourceTypes();
                        foreach (var sourceType in sources)
                        {
                            var csnMsg = new CanMessage(CanMessageType.Data, device.DeviceId, [sourceType]);
                            _canService.SendCanMessage(csnMsg);
                            await Task.Delay(100);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
                var minutes = 1;
                await Task.Delay(60 * 1000 * minutes, stoppingToken);
            }
        }
    }
}
