using Microsoft.AspNetCore.Mvc;
using SE.Common.DTO;
using SE.Service.Services;

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

        // GET: activity-management
        [HttpGet]
        public async Task<IActionResult> GetAllActivities([FromQuery] string day)
        {
            var result = await _activityService.GetAllScheduleForDay(day);
            return Ok(result);
        }

        // POST: activity-management
        [HttpPost]
        public async Task<IActionResult> CreateActivityWithSchedule([FromBody] CreateActivityModel model)
        {
            var result = await _activityService.CreateActivityWithSchedule(model);
            return Ok(result);
        }

        // PUT: activity-management/update
        [HttpPut("update")]
        public async Task<IActionResult> UpdateActivity([FromBody] UpdateScheduleModel model)
        {
            var result = await _activityService.UpdateSchedule(model);
            return Ok(result);
        }

        // GET: activity-management/{id}
        [HttpGet("{activityId}")]
        public async Task<IActionResult> GetActivityById(int activityId)
        {
            var result = await _activityService.GetActivityById(activityId);
            return Ok(result);
        }
    }
}
