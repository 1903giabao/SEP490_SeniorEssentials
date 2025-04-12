using Microsoft.AspNetCore.Mvc;
using SE.Common.Request.Account;
using SE.Common.Response.Profile;
using SE.Service.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace SE.API.Controllers
{
    [Route("profile-management")]
    [ApiController]
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet("{accountId}")]
        public async Task<IActionResult> GetDetailProfile([FromRoute] int accountId)
        {
            var result = await _profileService.GetDetailProfile(accountId);
            return Ok(result);
        }        
        
        [HttpPut()]
        public async Task<IActionResult> EditDetailProfile([FromForm] EditProfileRequest req)
        {
            var result = await _profileService.EditDetailProfile(req);
            return Ok(result);
        }

        [HttpGet("elderly/{elderlyId}")]
        public async Task<IActionResult> GetElderlyProfile([FromRoute] int elderlyId)
        {
            var result = await _profileService.GetElderlyProfile(elderlyId);
            return Ok(result);
        }

        [HttpPut("elderly")]
        public async Task<IActionResult> EditElderlyProfile([FromBody] EditElderlyProfile req)
        {
            var result = await _profileService.EditElderlyProfile(req);
            return Ok(result);
        }
    }
}
