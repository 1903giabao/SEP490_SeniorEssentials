using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SE.Common.Request;
using SE.Service.Services;
using SE.Common.DTO;
using SE.Common;
using Org.BouncyCastle.Ocsp;
using SE.Data.Models;
using System.IdentityModel.Tokens.Jwt;

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

        [HttpPost("managed-auths/otp/send")]
        public async Task<IActionResult> SendOTP(string account, string password, int role)
        {
            var result = await _identityService.SendOtpToUser(account, password, role);
            bool isSuccess = result.Data != null && result.Message == Const.SUCCESS_CREATE_MSG; 

            var response = new
            {
                isSuccess = isSuccess,
                data = result.Data
            };

            return Ok(response);
        }

        [HttpPost("managed-auths/otp/verify")]
        public async Task<IActionResult> SubmitOTP(CreateUserRequest req)
        {
            var result = await _identityService.SubmitOTP(req);
            bool isSuccess = result.Data != null && result.Message == Const.SUCCESS_CREATE_MSG;

            var response = new
            {
                isSuccess = isSuccess,
                data = result.Data
            };

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("managed-auths/sign-ups")]
        public async Task<IActionResult> Signup( SignUpModel req)
        {
            var result = await _identityService.Signup(req);
            bool isSuccess = result.Data != null && result.Message == Const.SUCCESS_CREATE_MSG; 

            var response = new
            {
                isSuccess = isSuccess,
                data = result.Data
            };

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("managed-auths/sign-ins")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var loginResult = await _identityService.Login(req.Email, req.Password, req.DeviceToken);
            bool isSuccess = loginResult.Data != null && loginResult.Message == Const.SUCCESS_LOGIN_MSG;

            var response = new
            {
                isSuccess = isSuccess,
                data = loginResult.Data
            };

            return Ok(response);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> CheckToken()
        {
            if (!Request.Headers.TryGetValue("Authorization", out var token))
            {
                return StatusCode(404, "Cannot find user");
            }
            token = token.ToString().Split()[1];
            var currentUser = await _identityService.GetUserInToken(token);
            if (currentUser == null)
            {
                return StatusCode(404, "Cannot find user");
            }
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new   ("Authorization header is missing or invalid.");
            }
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
       
            string email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

            var user = await _identityService.GetUserByEmail(email);
            if (user.Data == null)
            {
                return BadRequest("username is in valid");
            }

            return Ok(new
            {
                user = user.Data 
            });
        }


    }
}