﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Devices;

namespace Core.ControlDevices
{
    public abstract class ControlDevice : Device
    {
        public enum ControlDeviceState
        {
            Idle = 0,
            WaitingForResponse = 1,
            PrimaryActionRunning = 2,
        }

        public ControlDeviceState DeviceState { get; private set; }
        public override uint State => (uint)DeviceState;
        public DateTime? ActionDurationStopTime { get; private set; }

        protected ControlDevice(DeviceType type, byte deviceId, uint uniqueId, string deviceName) : base(type, deviceId, uniqueId, deviceName)
        {
            DeviceState = ControlDeviceState.Idle;
        }

        protected void SetState(ControlDeviceState state)
        {
            DeviceState = state;
        }

        public void SetIsWaitingForResponse()
        {
            DeviceState = ControlDeviceState.WaitingForResponse;
        }

        public void SetActionDurationStopTime(DateTime stopTime)
        {
            if(stopTime < DateTime.UtcNow)
            {
                throw new ArgumentException("Action stop time cannot be in the past!");
            }

            ActionDurationStopTime = stopTime;
        }

        public void ClearActionDurationStopTime()
        {
            ActionDurationStopTime = null;
        }
    }
}
