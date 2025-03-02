using Microsoft.AspNetCore.Mvc;
using SE.Common.DTO;
using SE.Service.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace SE.API.Controllers
{
    [Route("activity-management")]
    [ApiController]
    public class ActivityController : ControllerBase
    {
        private readonly IActivityService _activityService;

        public ActivityController(IActivityService activityService)
        {
            _activityService = activityService;
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Get all activities of elderly in day")]

        public async Task<IActionResult> GetAllActivities([FromQuery] DateOnly day, int AccountId)
        {
            var result = await _activityService.GetAllActivityForDay(AccountId, day);
            return Ok(result);
        }

        
        
        [HttpPost]
        [SwaggerOperation(Summary = "Create new activity")]

        public async Task<IActionResult> CreateActivityWithSchedule([FromBody] CreateActivityModel model)
        {
            var result = await _activityService.CreateActivityWithSchedules(model);
            return Ok(result);
        }

        [HttpPut("update")]
        [SwaggerOperation(Summary = "Update an activity")]

        public async Task<IActionResult> UpdateActivity([FromBody] UpdateScheduleModel model)
        {
            var result = await _activityService.UpdateActivityWithSchedules(model);
            return Ok(result);
        }

        [HttpPut("update/status")]
        [SwaggerOperation(Summary = "Update an activity status")]

        public async Task<IActionResult> UpdateActivity([FromQuery] int activityID, DateOnly date)
        {
            var result = await _activityService.UpdateStatusActivity(activityID, date);
            return Ok(result);
        }

    }
}
