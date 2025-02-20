using Microsoft.AspNetCore.Mvc;
using SE.Common;
using SE.Common.Request;
using SE.Service.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace SE.API.Controllers
{
    [Route("user-link-management")]
    [ApiController]
    public class UserLinkController : Controller
    {
        private readonly IUserLinkService _userLinkService;

        public UserLinkController(IUserLinkService userLinkService)
        {
            _userLinkService = userLinkService;
        }

        [HttpGet("request/{requestUserId}")]
        [SwaggerOperation(Summary = "Get all add friend request by request user")]

        public async Task<IActionResult> GetAllByRequestUserId(int requestUserId)
        {
            var result = await _userLinkService.GetAllByRequestUserId(requestUserId);

            return Ok(result);
        }

        [HttpGet("response/{responseUserId}")]
        [SwaggerOperation(Summary = "Get all add friend request by response user")]

        public async Task<IActionResult> GetAllByResponseUserId(int responseUserId)
        {
            var result = await _userLinkService.GetAllByResponseUserId(responseUserId);

            return Ok(result);
        }        
        
        [HttpPost("add-friend")]
        [SwaggerOperation(Summary = "Send add friend request")]

        public async Task<IActionResult> SendAddFriend([FromBody] SendAddFriendRequest req)
        {
            var result = await _userLinkService.SendAddFriend(req);

            return Ok(result);
        }

        [HttpPut("response-add-friend")]
        [SwaggerOperation(Summary = "Response the add friend request")]

        public async Task<IActionResult> ResponseAddFriend([FromBody] ResponseAddFriendRequest req)
        {
            var result = await _userLinkService.ResponseAddFriend(req);

            return Ok(result);
        }
    }
}
