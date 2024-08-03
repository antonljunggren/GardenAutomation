using Core.ControlDevices.WaterPump.Commands;
using Core.Shared;
using Core.Shared.Commands;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.BackgroundServices
{
    internal sealed class SystemSchedulingService : BackgroundService
    {
        private readonly ISystemRepository _systemRepository;
        private readonly ICommandDispatcher _commandDispatcher;

        public SystemSchedulingService(ISystemRepository systemRepository, ICommandDispatcher commandDispatcher)
        {
            _systemRepository = systemRepository;
            _commandDispatcher = commandDispatcher;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(1000 * 30, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                var system = await _systemRepository.GetSettingsAsync();
                var finishedCommands = new List<ScheduledCommand>();

                foreach(var scheduledCmd in system.ScheduledCommands)
                {
                    if (DateTime.UtcNow.AddMinutes(1) > scheduledCmd.StopTime)
                    {
                        try
                        {
                            if (scheduledCmd.CommandType == typeof(ToggleWaterPumpCommand))
                            {
                                var deviceId = Byte.Parse(scheduledCmd.Args[0].ToString()!);
                                var cmdImpl = new ToggleWaterPumpCommand(deviceId, (bool)scheduledCmd.Args[1]);
                                await _commandDispatcher.Dispatch<ToggleWaterPumpCommand, NoResult>(cmdImpl, stoppingToken);
                                finishedCommands.Add(scheduledCmd);
                                continue;
                            }

                            throw new Exception($"Command type {scheduledCmd.CommandType} is not supported in system scheduling service");
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e);
                        }
                    }
                }

                system = await _systemRepository.GetSettingsAsync();

                system.ScheduledCommands.RemoveAll(s => finishedCommands.Any(f => f.Equals(s)));

                await _systemRepository.UpdateSettings(system);

                await Task.Delay(1000*60, stoppingToken);
            }

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
