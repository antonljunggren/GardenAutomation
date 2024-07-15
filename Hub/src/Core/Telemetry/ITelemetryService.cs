﻿using Core.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Telemetry
{
    public interface ITelemetryService
    {
        Task SendMeasuredData(MeasuredDataPoint dataPoint);
        Task SendDeviceState(string state);
    }
}
