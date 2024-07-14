using Iot.Device.Button;
using Iot.Device.CharacterLcd;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.BackgroundServices
{
    internal sealed class LcdScreenService : BackgroundService
    {
        private const int buttonPin = 21;

        private const int lcdRegistreSelectPin = 20;
        private const int lcdReadWritePin = 16;
        private const int lcdEnablePin = 12;

        private const int lcdData4Pin = 26;
        private const int lcdData5Pin = 19;
        private const int lcdData6Pin = 13;
        private const int lcdData7Pin = 6;

        private const int lcdGroundPin = 5;

        private TimeSpan lcdOnTime = TimeSpan.FromSeconds(15);
        private DateTime lcdStartTime = DateTime.Today;
        private GpioController gpioController;
        private LcdInterface lcdInterface;
        private Hd44780 lcdDevice;

        public LcdScreenService()
        {
            gpioController = new GpioController();
            lcdInterface = LcdInterface.CreateGpio(lcdRegistreSelectPin, lcdEnablePin,
                [lcdData4Pin, lcdData5Pin, lcdData6Pin, lcdData7Pin], readWritePin: lcdReadWritePin);
            lcdDevice = new Hd44780(new System.Drawing.Size(20, 4), lcdInterface);

            gpioController.OpenPin(buttonPin, PinMode.InputPullUp);
            gpioController.OpenPin(lcdGroundPin, PinMode.Output);
            gpioController.Write(lcdGroundPin, PinValue.High);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var button = new GpioButton(buttonPin, gpio: gpioController, debounceTime: TimeSpan.FromMilliseconds(500));

            button.Press += (object? sender, EventArgs e) =>
            {
                lcdStartTime = DateTime.Now;
                StartLcd();
            };

            _ = Task.Run(async () =>
            {
                while(!stoppingToken.IsCancellationRequested)
                {
                    if(DateTime.Now - lcdStartTime > lcdOnTime)
                    {
                        StopLcd();
                    }
                    await Task.Delay(1000);
                }
            }, stoppingToken);

            await Task.Delay(Timeout.Infinite, stoppingToken);

            button.Dispose();
            lcdDevice.Dispose();
            lcdInterface.Dispose();
            gpioController.Dispose();
        }

        private void StartLcd()
        {
            gpioController.Write(lcdGroundPin, PinValue.Low);
            lcdDevice.DisplayOn = true;

            lcdDevice.Clear();
            lcdDevice.Home();
            lcdDevice.Write("Web=http://IP:PORT");
            lcdDevice.SetCursorPosition(0, 1);

            var host = Dns.GetHostEntry(Dns.GetHostName());
            var ssid = GetCurrentSSID();
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    if (!IPAddress.IsLoopback(ip) && ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        lcdDevice.Write($"IP={ip.ToString()}");
                        lcdDevice.SetCursorPosition(0, 2);
                        lcdDevice.Write($"PORT={8080}");
                        break;
                    }

                }
            }

            lcdDevice.SetCursorPosition(0, 3);
            lcdDevice.Write($"WIFI={ssid}");
        }

        private void StopLcd()
        {
            lcdDevice.DisplayOn = false;
            gpioController.Write(lcdGroundPin, PinValue.High);
        }

        private string GetCurrentSSID()
        {
            string ssid = "";
            try
            {
                // Create process start info for executing iwgetid
                ProcessStartInfo psi = new ProcessStartInfo("iwgetid", "-r");
                psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;

                // Start the process
                using (Process process = Process.Start(psi)!)
                {
                    // Read the output (SSID)
                    using (System.IO.StreamReader reader = process.StandardOutput)
                    {
                        ssid = reader.ReadToEnd().Trim();
                    }
                }
            }
            catch (Exception)
            {
                return "ERR";
            }

            return ssid;
        }
    }
}
