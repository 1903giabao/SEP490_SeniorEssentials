using Microsoft.AspNetCore.Mvc;
using SE.Common.DTO;
using SE.Common.Request;
using SE.Service.Services;
using System.Threading.Tasks;

namespace SE.API.Controllers
{
    [Route("iot-device-management")]
    [ApiController]
    public class IotDeviceController : ControllerBase
    {
        private readonly IIotDeviceService _iotDeviceService;

        public IotDeviceController(IIotDeviceService iotDeviceService)
        {
            _iotDeviceService = iotDeviceService;
        }

        // POST: iot-device-management
        [HttpPost]
        public async Task<IActionResult> CreateIotDevice([FromBody] CreateIotDeviceRequest req)
        {
            var result = await _iotDeviceService.CreateIotDevice(req);
            return Ok(result);
        }

    /*    // PUT: iot-device-management/{deviceId}
        [HttpPut("{deviceId}")]
        public async Task<IActionResult> UpdateIotDevice([FromRoute] int deviceId, [FromBody] CreateIotDeviceRequest req)
        {
            var result = await _iotDeviceService.UpdateIotDevice(deviceId, req);
            return Ok(result);
        }*/

        // GET: iot-device-management
        [HttpGet]
        public async Task<IActionResult> GetAllIotDevices()
        {
            var result = await _iotDeviceService.GetAllIotDevices();
            return Ok(result);
        }

        // GET: iot-device-management/{id}
        [HttpGet("{deviceId}")]
        public async Task<IActionResult> GetIotDeviceById(int deviceId)
        {
            var result = await _iotDeviceService.GetIotDeviceById(deviceId);
            return Ok(result);
        }

        // PUT: iot-device-management/update-status/{deviceId}
        [HttpPut("update-status/{deviceId}")]
        public async Task<IActionResult> UpdateIotDeviceStatus(int deviceId, [FromBody] string status)
        {
            var result = await _iotDeviceService.UpdateIotDeviceStatus(deviceId, status);
            return Ok(result);
        }
    }
}