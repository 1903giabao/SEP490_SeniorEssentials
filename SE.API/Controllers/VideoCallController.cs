using Microsoft.AspNetCore.Mvc;
using SE.Common.Request;
using SE.Service.Services;

namespace SE.API.Controllers
{
    [Route("video-call-management")]
    [ApiController]
    public class VideoCallController : Controller
    {
        private readonly IVideoCallService _videoCallService;

        public VideoCallController(IVideoCallService videoCallService)
        {
            _videoCallService = videoCallService;
        }

        [HttpPost]
        public async Task<IActionResult> MakeAVideoCall([FromBody] VideoCallRequest req)
        {
            var result = await _videoCallService.VideoCall(req);
            return Ok(result);
        }
    }
}
