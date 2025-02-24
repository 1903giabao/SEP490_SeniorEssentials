/*using Microsoft.AspNetCore.Mvc;
using SE.Common.DTO;
using SE.Common.Request;
using SE.Service.Services;
using System.Threading.Tasks;

namespace SE.API.Controllers
{
    [Route("health-indicators")]
    [ApiController]
    public class HealthIndicatorController : ControllerBase
    {
        private readonly IHealthIndicatorService _healthIndicatorService;

        public HealthIndicatorController(IHealthIndicatorService healthIndicatorService)
        {
            _healthIndicatorService = healthIndicatorService;
        }

        // POST: health-indicators
        [HttpPost]
        public async Task<IActionResult> CreateHealthIndicator([FromBody] CreateHealthIndicatorRequest request)
        {
            var result = await _healthIndicatorService.CreateHealthIndicator(request);
            return Ok(result);
        }

        // GET: health-indicators/elderly/{elderlyId}
        [HttpGet("elderly/{elderlyId}")]
        public async Task<IActionResult> GetAllHealthIndicatorsByElderlyId(int elderlyId)
        {
            var result = await _healthIndicatorService.GetAllHealthIndicatorsByElderlyId(elderlyId);
            return Ok(result);
        }

        // GET: health-indicators/{healthIndicatorId}
        [HttpGet("{healthIndicatorId}")]
        public async Task<IActionResult> GetHealthIndicatorById(int healthIndicatorId)
        {
            var result = await _healthIndicatorService.GetHealthIndicatorById(healthIndicatorId);
            return Ok(result);
        }

        // PUT: health-indicators/update
        [HttpPut("update")]
        public async Task<IActionResult> UpdateHealthIndicator([FromBody] UpdateHealthIndicatorRequest request)
        {
            var result = await _healthIndicatorService.UpdateHealthIndicator(request);
            return Ok(result);
        }

        // PUT: health-indicators/status/{healthIndicatorId}
        [HttpPut("status/{healthIndicatorId}")]
        public async Task<IActionResult> UpdateHealthIndicatorStatus(int healthIndicatorId)
        {
            var result = await _healthIndicatorService.UpdateHealthIndicatorStatus(healthIndicatorId);
            return Ok(result);
        }
    }
}*/