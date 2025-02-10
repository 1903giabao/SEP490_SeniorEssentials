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
        [SwaggerOperation(Summary = "Get all activities of elderly in day")]

        public async Task<IActionResult> GetallUsers()
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
    }
}
