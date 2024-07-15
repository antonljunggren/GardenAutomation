using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Core.Shared;
using Core.Telemetry;
using Core.Sensors;
using Core.Devices.ControlDevices;

namespace Infrastructure.BackgroundServices
{
    internal sealed class SystemTelemetryService : BackgroundService
    {
        private readonly ISystemRepository _systemRepository;
        private readonly ITelemetryService _telemetryService;

        private readonly ISensorRepository _sensorRepository;
        private readonly IControlDeviceRepository _controlDeviceRepository;

        public SystemTelemetryService(ISystemRepository systemRepository, ITelemetryService telemetryService, ISensorRepository sensorRepository, IControlDeviceRepository controlDeviceRepository)
        {
            _systemRepository = systemRepository;
            _telemetryService = telemetryService;
            _sensorRepository = sensorRepository;
            _controlDeviceRepository = controlDeviceRepository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //Send telemetry data regularly
            //or maybe just send it in the application layer, like when data from sensors come in...??

            await Task.Delay(15000, stoppingToken);
            Debug.WriteLine("Starting background task to send telemetry data");

            try
            {
                while(!stoppingToken.IsCancellationRequested)
                {
                    var sensors = await _sensorRepository.GetAll();
                    var devices = await _controlDeviceRepository.GetAll();

                    foreach(var sensor in sensors)
                    {
                        if(sensor.LastMeasuredDataPoints.Count > 0)
                        {
                            foreach(var dataPoint in sensor.LastMeasuredDataPoints)
                            {
                                await _telemetryService.SendMeasuredData(dataPoint);
                            }
                        }
                    }

                    foreach (var device in devices)
                    {
                        if (device.LastMeasuredDataPoints.Count > 0)
                        {
                            foreach (var dataPoint in device.LastMeasuredDataPoints)
                            {
                                await _telemetryService.SendMeasuredData(dataPoint);
                            }
                        }
                    }

                    await Task.Delay(1000 * 60 * 5, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
