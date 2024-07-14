using Iot.Device.DHTxx;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;

namespace Infrastructure.BackgroundServices
{
    internal sealed class CoolingFanControlService : BackgroundService
    {
        TimeOnly[] _scheduledFanTimes = [TimeOnly.Parse("07:00"), TimeOnly.Parse("19:00:00"), TimeOnly.Parse("12:00:00")];

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            const int fanControlPin = 14;
            const double fanTemperatureTrigger = 60.0d;

            bool fanIsOn = false;
            TimeSpan fanTimeOn = TimeSpan.FromMinutes(5);

            using var gpioController = new GpioController();
            gpioController.OpenPin(fanControlPin, PinMode.Output);
            gpioController.Write(fanControlPin, PinValue.Low);

            await Task.Delay(2000);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Path to the temperature file
                    string tempFilePath = "/sys/class/thermal/thermal_zone0/temp";

                    // Read the temperature value
                    string tempValue = File.ReadAllText(tempFilePath);

                    // Convert the value from millidegrees Celsius to degrees Celsius
                    double temperature = Convert.ToDouble(tempValue) / 1000.0;

                    if(temperature > fanTemperatureTrigger && !fanIsOn)
                    {
                        Debug.WriteLine($"Starting fan as temperature is {temperature:F2} °C");

                        fanIsOn = true;
                        gpioController.Write(fanControlPin, PinValue.High);
                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(fanTimeOn);
                            fanIsOn = false;
                            gpioController.Write(fanControlPin, PinValue.Low);
                        }, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"An error occurred while reading the temperature: {ex.Message}");
                }

                if (ToStartFanFromSchedule() && !fanIsOn)
                {
                    Debug.WriteLine($"Starting fan scheduled {DateTime.Now}");

                    fanIsOn = true;
                    gpioController.Write(fanControlPin, PinValue.High);
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(fanTimeOn);
                        fanIsOn = false;
                        gpioController.Write(fanControlPin, PinValue.Low);
                    }, stoppingToken);

                }

                await Task.Delay(1000 * 30, stoppingToken);
            }

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private bool ToStartFanFromSchedule()
        {
            var now = DateTime.Now;

            foreach(var time in _scheduledFanTimes)
            {
                var diff = TimeOnly.FromDateTime(now) - time;

                if(diff > TimeSpan.FromSeconds(10) && diff < TimeSpan.FromMinutes(5) && TimeOnly.FromDateTime(now) > time)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
