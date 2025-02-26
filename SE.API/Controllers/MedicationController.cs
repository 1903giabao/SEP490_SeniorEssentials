using Microsoft.AspNetCore.Mvc;
using SE.Common.Request;
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

        [HttpPost()]
        public async Task<IActionResult> CreateNewMedicationByManually([FromBody] CreateMedicationRequest medicationRequest)
        {
            var result = await _medicationService.CreateMedicationByManually(medicationRequest);
            return Ok(result);
        }

        [HttpGet("{elderlyId}/date")]
        public async Task<IActionResult> GetAll([FromRoute] int elderlyId, DateOnly day)
        {
            var result = await _medicationService.GetMedicationsForToday(elderlyId, day);
            return Ok(result);
        }


        [HttpGet("prescription/{elderlyId}")]
        public async Task<IActionResult> GetPrescriptionOfElderly([FromRoute] int elderlyId)
        {
            var result = await _medicationService.GetPrescriptionOfElderly(elderlyId);
            return Ok(result);
        }

        [HttpPut("prescription/{prescriptionId}")]
        public async Task<IActionResult> UpdateMediInPrescription(int prescriptionId, UpdateMedicationInPrescriptionRequest req)
        {
            var result = await _medicationService.UpdateMedicationInPrescription(prescriptionId,req);
            return Ok(result);
        }

        [HttpPut("confirm")]
        public async Task<IActionResult> ConfirmMedicationDrinking( ConfirmMedicationDrinkingReq req)
        {
            var result = await _medicationService.ConfirmMedicationDrinking(req);
            return Ok(result);
        }

        [HttpPut("cancel/{prescriptionId}")]
        public async Task<IActionResult> CancelPrescription([FromRoute]int prescriptionId)
        {
            var result = await _medicationService.CancelPrescription(prescriptionId);
            return Ok(result);
        }
    }
}