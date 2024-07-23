﻿using Core.ControlDevices.WaterPump.Commands;
using Core.Devices;
using Core.Devices.Shared.Commands;
using Core.Devices.Shared.Queries;
using Core.Shared.CAN;
using Core.Shared.Commands;
using Core.Shared.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.AccessControl;
using static Core.Devices.Device;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly ICanService _canService;

        public DeviceController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher, ICanService canService)
        {
            _queryDispatcher = queryDispatcher;
            _commandDispatcher = commandDispatcher;
            _canService = canService;
        }

        [HttpGet]
        public async Task<IEnumerable<Device>>GetAll()
        {
            var query = new GetAllDevicesQuery();
            return await _queryDispatcher.Dispatch<GetAllDevicesQuery, List<Device>>(query, CancellationToken.None);
        }

        [HttpPost("{deviceId}/name/{name}")]
        public async Task<IActionResult> TrySetState(byte deviceId, string name)
        {
            var cmd = new ChangeDeviceNameCommand(deviceId, name);
            await _commandDispatcher.Dispatch<ChangeDeviceNameCommand, NoResult>(cmd, CancellationToken.None);

            return Ok();
        }

        [HttpPost("waterpump/{deviceId}/toggle/{toggle}")]
        public async Task<IActionResult> TrySetWaterPumpPumping(byte deviceId, bool toggle)
        {
            var cmd = new ToggleWaterPumpCommand(deviceId, toggle);
            await _commandDispatcher.Dispatch<ToggleWaterPumpCommand, NoResult>(cmd, CancellationToken.None);

            return Ok();
        }

        [HttpPost("waterpump/{deviceId}/calibrate/{levelType}")]
        public async Task<IActionResult> CalibrateWaterLevel(byte deviceId, CalibrateWaterLevelCommand.WaterLevelCalibrationType levelType)
        {
            var cmd = new CalibrateWaterLevelCommand(deviceId, levelType);
            await _commandDispatcher.Dispatch<CalibrateWaterLevelCommand, NoResult>(cmd, CancellationToken.None);

            await Task.Delay(500);

            return Ok();
        }

        [HttpPost("{deviceId}/data/{dataSource}")]
        public async Task<IActionResult> RefreshDeviceData(byte deviceId, byte dataSource)
        {
            var csnMsg = new CanMessage(CanMessageType.Data, deviceId, [dataSource]);
            _canService.SendCanMessage(csnMsg);

            await Task.Delay(800);

            return Ok();
        }
    }
}
