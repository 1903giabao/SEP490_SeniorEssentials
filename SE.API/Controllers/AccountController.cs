using Microsoft.AspNetCore.Mvc;
using SE.Common;
using SE.Common.Request.Account;
using SE.Service.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace SE.API.Controllers
{
    [Route("account-management")]
    [ApiController]
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService activityService)
        {
            _accountService = activityService;
        }

        [HttpGet("{roleId}")]
        [SwaggerOperation(Summary = "Get all user")]

        public async Task<IActionResult> GetAllUsers(int roleId = 0)
        {
            var result = await _accountService.GetAllUsers(roleId);
            return Ok(result);
        }

        [HttpGet("userId")]
        [SwaggerOperation(Summary = "Get detail user by id")]

        public async Task<IActionResult> GetUserById(int userId)
        {
            var result = await _accountService.GetUserById(userId);
            return Ok(result);
        }

        [HttpGet("phoneNumber/{phoneNumber}/{userId}")]
        [SwaggerOperation(Summary = "Get detail user by phone number")]

        public async Task<IActionResult> GetUserByPhoneNumber(string phoneNumber, int userId)
        {
            var result = await _accountService.GetUserByPhoneNumber(phoneNumber, userId);
            return Ok(result);
        }

        [HttpPost("system-account")]
        public async Task<IActionResult> CreateSystemAccount([FromBody] CreateSystemAccountRequest req)
        {
            var result = await _accountService.CreateSystemAccount(req);
            return Ok(result);
        }        
        
        [HttpPost("professor-account")]
        public async Task<IActionResult> CreateProfessorAccount([FromForm] CreateProfessorAccountRequest req)
        {
            var result = await _accountService.CreateProfessorAccount(req);
            return Ok(result);
        }
    }
}
