using Microsoft.AspNetCore.Mvc;
using SE.Service.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace SE.API.Controllers
{
    [Route("medication-management")]
    [ApiController]
    public class MedicationController : ControllerBase
    {
        private readonly IMedicationService _medicationService;

        public MedicationController(IMedicationService medicationService)
        {
            _medicationService = medicationService;
        }

        [HttpPost("scan")]
        public async Task<IActionResult> Scan(IFormFile picture, int ElderlyId)
        {
            var result = await _medicationService.ScanFromPic(picture, ElderlyId);
            return Ok(result);
        }

        [HttpGet("{elderlyId}/Date")]
        public async Task<IActionResult> GetAll([FromRoute] int elderlyId, DateTime day)
        {
            var result = await _medicationService.GetMedicationsForToday(elderlyId, day);
            return Ok(result);
        }
    }
}
