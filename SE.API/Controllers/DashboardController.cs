using Microsoft.AspNetCore.Mvc;
using SE.Service.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace SE.API.Controllers
{
    [Route("dashboard-management")]
    [ApiController]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("admin-dashboard")]

        public async Task<IActionResult> AdminDashboard()
        {
            var result = await _dashboardService.AdminDashboard();
            return Ok(result);
        }
    }
}
