﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using SE.Common.Request;
using SE.Common.Request.Professor;
using SE.Common.Request.Subscription;
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

        [HttpGet("number-of-meeting/elderly/{elderlyId}")]
        public async Task<IActionResult> GetNumberOfMeetingLeftByElderly([FromRoute] int elderlyId)
        {
            var result = await _professorScheduleService.GetNumberOfMeetingLeftByElderly(elderlyId);
            return Ok(result);
        }        
        
        [HttpGet("elderly/{professorId}")]
        public async Task<IActionResult> GetListElderlyByProfessorId([FromRoute] int professorId)
        {
            var result = await _professorScheduleService.GetListElderlyByProfessorId(professorId);
            return Ok(result);
        }

        [HttpPut("user-subscription-professor")]
        public async Task<IActionResult> AddProfessorToSubscriptionByElderly([FromBody] AddProfessorToSubscriptionRequest req)
        {
            var result = await _professorScheduleService.AddProfessorToSubscriptionByElderly(req);
            return Ok(result);
        }        
        
        [HttpPost("feedback")]
        public async Task<IActionResult> GiveProfessorFeedbackByAccount([FromBody] GiveProfessorFeedbackByAccountVM req)
        {
            var result = await _professorScheduleService.GiveProfessorFeedbackByAccount(req);
            return Ok(result);
        }
        [HttpGet("feedback/{accountId}")]
        public async Task<IActionResult> GetAllProfessor([FromRoute] int accountId)
        {
            var result = await _professorScheduleService.GetAllRatingsByProfessorId(accountId);
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
        public async Task<IActionResult> GetTimeSlot(int professorId, DateTime date)
        {
            var result = await _professorScheduleService.GetTimeSlot(professorId, date);
            return Ok(result);
        }
        [HttpGet("time-slot/week")]
        public async Task<IActionResult> GetProfessorWeeklyTimeSlots(int accountId)
        {
            var result = await _professorScheduleService.GetProfessorWeeklyTimeSlots(accountId);
            return Ok(result);
        }


        [HttpPost("filter")]
        public async Task<IActionResult> GetFilteredProfessors([FromBody] FilterProfessorRequest request)
        {
            var result = await _professorScheduleService.GetFilteredProfessors(request);
            return Ok(result);

        }

        [HttpGet("elderly/schedules/{accountId}")]
        public async Task<IActionResult> GetProfessorSchedule([FromRoute]int accountId, string type, string date)
        {
            var rs = await _professorScheduleService.GetProfessorSchedule(accountId,type,date);
            return Ok(rs);
        }
        [HttpGet("appointment/{accountId}")]
        public async Task<IActionResult> GetScheduleOfElderlyByProfessorId(int accountId, string type, string date)
        {
            var rs = await _professorScheduleService.GetScheduleOfElderlyByProfessorId(accountId, type, date);
            return Ok(rs);
        }

        [HttpGet("schedule/{professorId}")]
        public async Task<IActionResult> GetProfessorScheduleInProfessor([FromRoute] int professorId)
        {
            var rs = await _professorScheduleService.GetProfessorScheduleInProfessor(professorId);
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
        [HttpGet("appointment")]
        public async Task<IActionResult> GetAllAppointment()
        {
            var rs = await _professorScheduleService.GetAllAppointment();
            return Ok(rs);
        }


        [HttpPut("cancel/{appoinmentId}/{accountId}")]
        public async Task<IActionResult> CancelMeeting([FromRoute] int appoinmentId, int accountId)
        {
            var rs = await _professorScheduleService.CancelMeeting(appoinmentId, accountId);
            return Ok(rs);
        }

        [HttpPut("confirmation")]
        public async Task<IActionResult> ConfirmMeeting( int appoinmentId,List<int> accountId)
        {
            var rs = await _professorScheduleService.ConfirmMeeting(appoinmentId, accountId);
            return Ok(rs);
        }

        [HttpPut("professor-detail")]
        public async Task<IActionResult> UpdateProfessorInfor([FromForm] UpdateProfessorRequest req)
        {
            var rs = await _professorScheduleService.UpdateProfessorInfor(req);
            return Ok(rs);
        }        
        
        [HttpPost("professor-appointment")]
        public async Task<IActionResult> BookProfessorAppointment([FromBody] BookProfessorAppointmentRequest req)
        {
            var rs = await _professorScheduleService.BookProfessorAppointment(req);
            return Ok(rs);
        }

        [HttpPost("professor-appointment/report")]
        public async Task<IActionResult> CreateAppointmentReport([FromBody] CreateReportRequest req)
        {
            var rs = await _professorScheduleService.CreateAppointmentReport(req);
            return Ok(rs);
        }        
        
        [HttpPost("professor-appointment/image")]
        public async Task<IActionResult> UploadAppointmentImage([FromForm] UploadAppointmentImageRequest req)
        {
            var rs = await _professorScheduleService.UploadAppointmentImage(req);
            return Ok(rs);
        }               
    }
}
