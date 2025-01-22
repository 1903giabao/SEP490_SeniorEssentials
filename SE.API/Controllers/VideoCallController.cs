using Microsoft.AspNetCore.Mvc;
using SE.Common.Request;
using SE.Service.Services;

namespace SE.API.Controllers
{
    public class VideoCallController : Controller
    {
        private readonly IVideoCallService _videoCallService;

        public VideoCallController(IVideoCallService videoCallService)
        {
            _videoCallService = videoCallService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllVideoCallHistory()
        {
            var result = await _videoCallService.GetAllVideoCallHistory();
            return Ok(result);
        }

        [HttpGet("{videoCallId}")]
        public async Task<IActionResult> GetAllVideoCallHistory([FromRoute] int videoCallId)
        {
            var result = await _videoCallService.GetVideoCallHistoryById(videoCallId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateVideoCall([FromBody] VideoCallRequest req)
        {
            var result = await _videoCallService.CreateVideoCall(req);
            return Ok(result);
        }
    }
}
