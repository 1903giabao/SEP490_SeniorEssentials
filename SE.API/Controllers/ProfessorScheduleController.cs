using Microsoft.AspNetCore.Mvc;
using SE.Common.Request;
using SE.Service.Services;

namespace SE.API.Controllers
{
    [Route("professor-schedule-management")]
    [ApiController]
    public class ProfessorScheduleController : Controller
    {
        private readonly IProfessorScheduleService _professorScheduleService;

        public ProfessorScheduleController(IProfessorScheduleService professorScheduleService)
        {
            _professorScheduleService = professorScheduleService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateSchedule([FromBody] List<ProfessorScheduleRequest> req)
        {
            var result = await _professorScheduleService.CreateSchedule(req);
            return Ok(result);
        }
    }
}
