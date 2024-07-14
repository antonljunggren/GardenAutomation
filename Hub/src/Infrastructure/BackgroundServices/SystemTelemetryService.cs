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

namespace Infrastructure.BackgroundServices
{
    internal sealed class SystemTelemetryService : BackgroundService
    {
        private readonly ISystemRepository _systemRepository;
        private readonly ITelemetryService _telemetryService;

        public SystemTelemetryService(ISystemRepository systemRepository, ITelemetryService telemetryService)
        {
            _systemRepository = systemRepository;
            _telemetryService = telemetryService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //Send telemetry data regularly
            //or maybe just send it in the application layer, like when data from sensors come in...??

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
