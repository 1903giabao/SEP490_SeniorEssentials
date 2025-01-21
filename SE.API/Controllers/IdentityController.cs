using Microsoft.AspNetCore.Mvc;
using SE.Service.Services;

namespace SE.API.Controllers
{


    [Route("auth-management")]
    [ApiController]
    public class IdentityController : Controller
    {
        private readonly IIdentityService _identityService;
        private readonly IEmailService _emailService;


        public IdentityController(IIdentityService identityService, IEmailService emailService)
        {
            _emailService = emailService;
            _identityService = identityService; 
        }
        [HttpPost]
        public async Task<IActionResult> SendOTP (string email)
        {
            var result  = await _identityService.SendOtpToUser(email);

            return Ok(result);
        }
    }
}
