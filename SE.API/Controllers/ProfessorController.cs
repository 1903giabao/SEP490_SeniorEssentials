using Microsoft.AspNetCore.Mvc;
using SE.Common.Request;
using SE.Common.Request.Professor;
using SE.Service.Services;

namespace SE.API.Controllers
{
    [Route("api/[controller]")]
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
        [HttpGet]
        public async Task<IActionResult> GetAllProfessor()
        {
            var result = await _professorScheduleService.GetAllProfessor();
            return Ok(result);
        }
        [HttpGet("/{professorId}")]
        public async Task<IActionResult> GetAllProfessor([FromRoute] int professorId)
        {
            var result = await _professorScheduleService.GetProfessorDetail(professorId);
            return Ok(result);
        }

        [HttpGet("/time-slot")]
        public async Task<IActionResult> GetAllProfessor(int professorId, DateOnly date)
        {
            var result = await _professorScheduleService.GetTimeSlot(professorId, date);
            return Ok(result);
        }

        [HttpPost("/filter")]
        public async Task<IActionResult> GetFilteredProfessors([FromBody] FilterProfessorRequest request)
        {
            var result = await _professorScheduleService.GetFilteredProfessors(
                request);
            return Ok(result);

        }

        [HttpGet("elderly/{accountId}/schedules")]
        public async Task<IActionResult> GetProfessorSchedule([FromRoute]int accountId, string type)
        {
            var rs = await _professorScheduleService.GetProfessorSchedule(accountId,type);
            return Ok(rs);
        }


    }
}
