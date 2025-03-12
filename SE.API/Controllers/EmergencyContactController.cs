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
        private static readonly Dictionary<int, CancellationTokenSource> CallCancellations = new();
        private static readonly Dictionary<int, string> CallStatuses = new();

        public EmergencyContactController(IEmergencyContactService emergencyContactService)
        {
            _emergencyContactService = emergencyContactService;
        }

        [HttpPost("family-emergency-call")]
        public async Task<IActionResult> FamilyEmergencyCall([FromQuery] int accountId)
        {
            return await HandleEmergencyCall(accountId, "Family");
        }

        [HttpPost("doctor-emergency-call")]
        public async Task<IActionResult> DoctorEmergencyCall([FromQuery] int accountId)
        {
            return await HandleEmergencyCall(accountId, "Doctor");
        }

        private async Task<IActionResult> HandleEmergencyCall(int accountId, string callType)
        {
            if (accountId <= 0)
            {
                return BadRequest("Invalid account ID.");
            }

            var cts = new CancellationTokenSource();
            CallCancellations[accountId] = cts;
            CallStatuses[accountId] = "Running";

            await Task.WhenAny(Task.Delay(5000));

            var cancelTask = WaitForUserCancellation(accountId, cts.Token);

            var completedTask = await Task.WhenAny(cancelTask, Task.Delay(5000)); 
            if (completedTask == cancelTask && cancelTask.Result)
            {
                cts.Cancel();
                CallStatuses[accountId] = "Cancelled";
                Ok(new { Status = 1, Message = "Emergency call cancelled by user." });
            }

            var callTask = RunEmergencyCall(accountId, callType, cts.Token);
            return await callTask;
        }

        [HttpPost("cancel/{accountId}")]
        public IActionResult CancelCall(int accountId)
        {
            if (CallStatuses.ContainsKey(accountId) && CallStatuses[accountId] == "Running")
            {
                CallStatuses[accountId] = "Cancelled";
                CallCancellations[accountId]?.Cancel();
                return Ok(new { accountId, Status = "Call cancelled successfully." });
            }
            return Ok(new { Status = 0, Message = "Call ID not found or already processed." });
        }

        private async Task<bool> WaitForUserCancellation(int accountId, CancellationToken token)
        {
            int attempts = 10;
            while (attempts-- > 0)
            {
                if (CallStatuses.ContainsKey(accountId) && CallStatuses[accountId] == "Cancelled")
                {
                    return true;
                }
                await Task.Delay(1000, token);
            }
            return false;
        }

        private async Task<IActionResult> RunEmergencyCall(int accountId, string callType, CancellationToken token)
        {
            try
            {
                CallStatuses[accountId] = "In Progress";

                object result = callType switch
                {
                    "Family" => await _emergencyContactService.FamilyEmergencyCall(accountId),
                    "Doctor" => await _emergencyContactService.DoctorEmergencyCall(accountId),
                    _ => throw new ArgumentException("Invalid call type")
                };

                CallStatuses[accountId] = "Completed";
                return Ok(result);
            }
            catch (TaskCanceledException)
            {
                CallStatuses[accountId] = "Cancelled";
                return BadRequest("Call was cancelled.");
            }
        }
    }

    // POST: emergency-contacts
    /*        [HttpPost]
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
            }*/
}