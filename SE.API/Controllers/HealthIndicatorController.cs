using Microsoft.AspNetCore.Mvc;
using SE.Service.Services;

namespace SE.API.Controllers
{
    [Route("health-indicator-management")]
    [ApiController]
    public class HealthIndicatorController : Controller
    {
        private readonly IHealthIndicatorService _healthIndicatorService;

        public HealthIndicatorController(IHealthIndicatorService healthIndicatorService)
        {
            _healthIndicatorService = healthIndicatorService;
        }
    }
}
