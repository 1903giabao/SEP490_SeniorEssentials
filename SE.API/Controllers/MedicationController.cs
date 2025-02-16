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

        [HttpPost]
        public async Task<IActionResult> Scan( IFormFile day)
        {
            var result = await _medicationService.ScanFromPic(day);
            return Ok(result);
        }
    }
}
