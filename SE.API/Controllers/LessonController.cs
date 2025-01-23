using Microsoft.AspNetCore.Mvc;
using SE.Common.Request;
using SE.Service.Services;

namespace SE.API.Controllers
{
    [Route("lesson-management")]
    [ApiController]
    public class LessonController : Controller
    {
        private readonly ILessonService _lessonService;

        public LessonController(ILessonService LessonService)
        {
            _lessonService = LessonService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLesson()
        {
            var result = await _lessonService.GetAllLesson();
            return Ok(result);
        }

        [HttpGet("{lessonId}")]
        public async Task<IActionResult> GetLessonById([FromRoute] int lessonId)
        {
            var result = await _lessonService.GetLessonById(lessonId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateLesson([FromBody] CreateLessonRequest req)
        {
            var result = await _lessonService.CreateLesson(req);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateLesson([FromQuery] int lessonId, [FromBody] CreateLessonRequest req)
        {
            var result = await _lessonService.UpdateLesson(lessonId, req);
            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteLesson([FromQuery] int lessonId)
        {
            var result = await _lessonService.DeleteLesson(lessonId);
            return Ok(result);
        }

        [HttpGet("feedback/{lessonId}")]
        public async Task<IActionResult> GetAllFeedbackByLessonId([FromRoute] int lessonId)
        {
            var result = await _lessonService.GetAllFeedbackByLessonId(lessonId);
            return Ok(result);
        }

        [HttpPost("feedback")]
        public async Task<IActionResult> Feedback([FromBody] LessonFeedbackRequest req)
        {
            var result = await _lessonService.Feedback(req);
            return Ok(result);
        }
    }
}
