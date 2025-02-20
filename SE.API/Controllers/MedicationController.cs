/*using Microsoft.AspNetCore.Mvc;
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

        [HttpPost]
        public async Task<IActionResult> Scan( IFormFile day, int ElderlyId)
        {
            var result = await _medicationService.ScanFromPic(day,ElderlyId);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int ElderlyId)
        {
            var result = await _medicationService.GetMedicationsForToday(ElderlyId);
            return Ok(result);
        }
    }
}
*/