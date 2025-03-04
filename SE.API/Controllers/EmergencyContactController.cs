using Google.Type;
using Microsoft.AspNetCore.Mvc;
using SE.Common.DTO;
using SE.Common.Request;
using SE.Service.Services;
using System.Threading.Tasks;

namespace SE.API.Controllers
{
    [Route("emergency-contacts")]
    [ApiController]
    public class EmergencyContactController : ControllerBase
    {
        private readonly IEmergencyContactService _emergencyContactService;

        public EmergencyContactController(IEmergencyContactService emergencyContactService)
        {
            _emergencyContactService = emergencyContactService;
        }

        [HttpGet("emergency-call/{phoneNumber}")]
        public async Task<IActionResult> ExecuteEmergencyCall(string phoneNumber)
        {
            var result = await _emergencyContactService.ExecuteEmergencyCall(new List<string>(), phoneNumber);
            return Ok(result);
        }

        // POST: emergency-contacts
        [HttpPost]
        public async Task<IActionResult> CreateEmergencyContact([FromBody] CreateEmergencyContactRequest request)
        {
            var result = await _emergencyContactService.CreateEmergencyContact(request);
            return Ok(result);
        }

        // PUT: emergency-contacts/update
        [HttpPut("update")]
        public async Task<IActionResult> UpdateEmergencyContact([FromBody] UpdateEmergencyContactRequest request)
        {
            var result = await _emergencyContactService.UpdateEmergencyContact(request);
            return Ok(result);
        }

        // GET: emergency-contacts/elderly/{elderlyId}
        [HttpGet("elderly/{elderlyId}")]
        public async Task<IActionResult> GetEmergencyContactsByElderlyId(int elderlyId)
        {
            var result = await _emergencyContactService.GetEmergencyContactsByElderlyId(elderlyId);
            return Ok(result);
        }

        // PUT: emergency-contacts/status/{emergencyContactId}
        [HttpPut("status/{emergencyContactId}")]
        public async Task<IActionResult> UpdateEmergencyContactStatus(int emergencyContactId)
        {
            var result = await _emergencyContactService.UpdateEmergencyContactStatus(emergencyContactId);
            return Ok(result);
        }
    }
}   