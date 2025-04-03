using Microsoft.AspNetCore.Mvc;
using SE.Common.DTO;
using SE.Common.Request.Report;
using SE.Service.Services;

namespace SE.API.Controllers
{
    [Route("report-management")]
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
    }
}
