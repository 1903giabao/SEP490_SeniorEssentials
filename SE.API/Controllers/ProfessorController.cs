using Microsoft.AspNetCore.Mvc;
using SE.Common.Request;
using SE.Service.Services;

namespace SE.API.Controllers
{
    [Route("professor-management")]
    [ApiController]
    public class ProfessorController : Controller
    {
        private readonly IProfessorService _professorScheduleService;

        public ProfessorController(IProfessorService professorScheduleService)
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
