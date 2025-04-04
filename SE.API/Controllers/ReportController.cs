using Microsoft.AspNetCore.Mvc;
using SE.Common.DTO;
using SE.Common.Request.Report;
using SE.Service.Services;

namespace SE.API.Controllers
{
    [Route("api/[controller]")]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateReport([FromForm] CreateReportRequest model)
        {
            var result = await _reportService.CreateReport(model);
            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _reportService.GetAll();
            return Ok(result);
        }
        [HttpGet("{accountId}")]
        public async Task<IActionResult> GetAllReportOfAccountId(int accountId)
        {
            var result = await _reportService.GetAllReportOfAccountId(accountId);
            return Ok(result);
        }
        // PUT: combo-management/update/{id}
        [HttpPut("update/{reportId}")]
        public async Task<IActionResult> UpdateComboStatus(int reportId)
        {
            var result = await _reportService.UpdateStatusReport(reportId);
            return Ok(result);
        }
    }
}
