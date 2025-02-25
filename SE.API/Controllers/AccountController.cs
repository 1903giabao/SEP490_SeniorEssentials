using Microsoft.AspNetCore.Mvc;
using SE.Common;
using SE.Service.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace SE.API.Controllers
{
    [Route("account-management")]
    [ApiController]
    public class AccountController : Controller
    {
        private readonly IAccountService _activityService;

        public AccountController(IAccountService activityService)
        {
            _activityService = activityService;
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Get all user")]

        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _activityService.GetAllUsers();
            bool isSuccess = result.Data != null && result.Message == Const.SUCCESS_READ_MSG;

            var response = new
            {
                isSuccess = isSuccess,
                data = result.Data
            };

            return Ok(result);
        }

        [HttpGet("userId")]
        [SwaggerOperation(Summary = "Get detail user by id")]

        public async Task<IActionResult> GetUserById(int userId)
        {
            var result = await _activityService.GetUserById(userId);
            bool isSuccess = result.Data != null && result.Message == Const.SUCCESS_READ_MSG;

            var response = new
            {
                isSuccess = isSuccess,
                data = result.Data
            };

            return Ok(result);
        }        
        
        [HttpGet("phoneNumber")]
        [SwaggerOperation(Summary = "Get detail user by phone number")]

        public async Task<IActionResult> GetUserByPhoneNumber(string phoneNumber)
        {
            var result = await _activityService.GetUserByPhoneNumber(phoneNumber);
            bool isSuccess = result.Data != null && result.Message == Const.SUCCESS_READ_MSG;

            var response = new
            {
                isSuccess = isSuccess,
                data = result.Data
            };

            return Ok(result);
        }
    }
}
