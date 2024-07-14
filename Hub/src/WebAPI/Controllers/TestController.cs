using Core.Devices;
using Core.Devices.Shared.Commands;
using Core.Shared.CAN;
using Core.Shared.Commands;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Core.Devices.MeasuredDataPoint;

namespace WebAPI.Controllers
{
#if DEBUG
    [Route("[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly ICanService _canService;

        public TestController(ICommandDispatcher commandDispatcher, ICanService canService)
        {
            _commandDispatcher = commandDispatcher;
            _canService = canService;
        }

        [HttpPost("{deviceId}/measure/{dataType}/{value}")]
        public async Task<IActionResult> TrySetState(byte deviceId, DataPointType dataType, int value)
        {
            var cmd = new AddMeasurementDataPointCommand(deviceId, value, 0);
            await _commandDispatcher.Dispatch< AddMeasurementDataPointCommand, NoResult>(cmd, CancellationToken.None);

            return Ok();
        }

        [HttpGet("{deviceId}/measure/")]
        public  IActionResult GetData(byte deviceId)
        {
            var msg = new CanMessage(CanMessageType.Data, 0, []);
            _canService.SendCanMessage(msg);

            return Ok();
        }

        [HttpPost("device/add")]
        public async Task<IActionResult> AddDevice([FromBody]RegisterDeviceCommand cmd)
        {
            var device = await _commandDispatcher.Dispatch<RegisterDeviceCommand, Device>(cmd, CancellationToken.None);

            return Ok(device);
        }
    }
#endif
}
