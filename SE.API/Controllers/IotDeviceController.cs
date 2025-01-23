using Microsoft.AspNetCore.Mvc;
using SE.Service.Services;

namespace SE.API.Controllers
{
    [Route("iot-device-management")]
    [ApiController]
    public class IotDeviceController : Controller
    {
        private readonly IIotDeviceService _iotDeviceService;

        public IotDeviceController(IIotDeviceService iotDeviceService)
        {
            _iotDeviceService = iotDeviceService;
        }
    }
}
