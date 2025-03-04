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
        public async Task<IActionResult> Scan(IFormFile file)
        {
            var result = await _medicationService.ScanByGoogle(file);
            return Ok(result);
        }

        [HttpPost()]
        public async Task<IActionResult> CreateNewMedicationByManually([FromBody] CreateMedicationRequest medicationRequest)
        {
            var result = await _medicationService.CreateMedicationByManually(medicationRequest);
            return Ok(result);
        }

        [HttpGet("{accountId}/date")]
        public async Task<IActionResult> GetAll([FromRoute] int accountId, DateOnly day)
        {
            var result = await _medicationService.GetMedicationsForToday(accountId, day);
            return Ok(result);
        }


        [HttpGet("prescription/{accountId}")]
        public async Task<IActionResult> GetPrescriptionOfElderly([FromRoute] int accountId)
        {
            var result = await _medicationService.GetPrescriptionOfElderly(accountId);
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