using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using SE.Common.Request;
using SE.Service.Services;
using SE.Common.DTO;

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

        [AllowAnonymous]
        [HttpPost("managed-auths/sign-ups")]
        public async Task<IActionResult> Signup([FromBody] ElderlySignUpModel req)
        {
           var result = await _identityService.SignupForElderly(req);
                  return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("managed-auths/sign-ins")]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel req)
        {
            var loginResult = await _identityService.Login(req.Email, req.Password, req.DeviceToken);
            var handler = new JwtSecurityTokenHandler();

            var rs = handler.WriteToken( (JwtSecurityToken)loginResult.Data);


            return Ok(rs);
        }

    }
}
