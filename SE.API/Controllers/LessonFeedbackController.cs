using Microsoft.AspNetCore.Mvc;
using SE.Common.Request;
using SE.Service.Services;

namespace SE.API.Controllers
{
    [Route("lesson-feedback-management")]
    [ApiController]
    public class LessonFeedbackController : Controller
    {
        private readonly ILessonFeedbackService _lessonFeedbackService;

        public LessonFeedbackController(ILessonFeedbackService lessonFeedbackService)
        {
            _lessonFeedbackService = lessonFeedbackService;
        }

        [HttpGet("{lessonId}")]
        public async Task<IActionResult> GetAllFeedbackByLessonId([FromRoute] int lessonId)
        {
            var result = await _lessonFeedbackService.GetAllFeedbackByLessonId(lessonId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Feedback([FromBody] LessonFeedbackRequest req)
        {
            var result = await _lessonFeedbackService.Feedback(req);
            return Ok(result);
        }
    }
}
