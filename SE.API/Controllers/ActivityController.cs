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

        public async Task<IActionResult> GetAllActivities([FromQuery] DateTime day)
        {
            var result = await _activityService.GetAllScheduleForDay(day);
            return Ok(result);
        }

        
        
        [HttpPost]
        [SwaggerOperation(Summary = "Create new activity")]

        public async Task<IActionResult> CreateActivityWithSchedule([FromBody] CreateActivityModel model)
        {
            var result = await _activityService.CreateActivityWithSchedule(model);
            return Ok(result);
        }

        [HttpPut("update")]
        [SwaggerOperation(Summary = "Update an activity")]

        public async Task<IActionResult> UpdateActivity([FromBody] UpdateScheduleModel model)
        {
            var result = await _activityService.UpdateSchedule(model);
            return Ok(result);
        }

        [HttpGet("{activityId}")]
        public async Task<IActionResult> GetActivityById(int activityId)
        {
            var result = await _activityService.GetActivityById(activityId);
            return Ok(result);
        }
    }
}
