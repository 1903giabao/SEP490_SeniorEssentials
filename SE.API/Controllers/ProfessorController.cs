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
        [HttpGet("{professorId}")]
        public async Task<IActionResult> GetProfessorDetail([FromRoute] int professorId)
        {
            var result = await _professorScheduleService.GetProfessorDetail(professorId);
            return Ok(result);
        }        
        [HttpGet("by-account/{accountId}")]
        public async Task<IActionResult> GetProfessorDetailByAccountId([FromRoute] int accountId)
        {
            var result = await _professorScheduleService.GetProfessorDetailByAccountId(accountId);
            return Ok(result);
        }

        [HttpGet("time-slot")]
        public async Task<IActionResult> GetAllProfessor(int professorId, DateOnly date)
        {
            var result = await _professorScheduleService.GetTimeSlot(professorId, date);
            return Ok(result);
        }

        [HttpPost("filter")]
        public async Task<IActionResult> GetFilteredProfessors([FromBody] FilterProfessorRequest request)
        {
            var result = await _professorScheduleService.GetFilteredProfessors(
                request);
            return Ok(result);

        }

        [HttpGet("elderly/schedules/{accountId}")]
        public async Task<IActionResult> GetProfessorSchedule([FromRoute]int accountId, string type)
        {
            var rs = await _professorScheduleService.GetProfessorSchedule(accountId,type);
            return Ok(rs);
        }
        [HttpGet("appointment/{accountId}")]
        public async Task<IActionResult> GetProfessorSchedule([FromRoute] int accountId)
        {
            var rs = await _professorScheduleService.GetScheduleOfElderlyByProfessorId(accountId);
            return Ok(rs);
        }

        [HttpGet("report/appointment/{appointmentId}")]
        public async Task<IActionResult> GetReportInAppointment([FromRoute] int appointmentId)
        {
            var rs = await _professorScheduleService.GetReportInAppointment(appointmentId);
            return Ok(rs);
        }

        [HttpGet("elderly/professor/detail/{elderlyId}")]
        public async Task<IActionResult> GetProfessorDetailOfElderly([FromRoute] int elderlyId)
        {
            var rs = await _professorScheduleService.GetProfessorDetailOfElderly(elderlyId);
            return Ok(rs);
        }


        [HttpPut("cancel/{appoinmentId}")]
        public async Task<IActionResult> CancelProfessorAppointment([FromRoute] int appoinmentId)
        {
            var rs = await _professorScheduleService.CancelProfessorAppointment(appoinmentId);
            return Ok(rs);
        }        
        
        [HttpPut("professor-detail")]
        public async Task<IActionResult> UpdateProfessorInfor([FromForm] UpdateProfessorRequest req)
        {
            var rs = await _professorScheduleService.UpdateProfessorInfor(req);
            return Ok(rs);
        }
    }
}
