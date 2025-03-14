using Google.Type;
using Microsoft.AspNetCore.Mvc;
using SE.Common.DTO;
using SE.Common.Request;
using SE.Common.Request.Emergency;
using SE.Service.Base;
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

        [HttpGet("list-emergency-information/{emergencyId}")]
        public async Task<IActionResult> GetListEmergencyInformation(int emergencyId)
        {
            var result = await _emergencyContactService.GetListEmergencyInformation(emergencyId);
            return Ok(result);
        }

        [HttpGet("newest-emergency-information/{emergencyId}")]
        public async Task<IActionResult> GetNewestEmergencyInformation(int emergencyId)
        {
            var result = await _emergencyContactService.GetNewestEmergencyInformation(emergencyId);
            return Ok(result);
        }

        [HttpGet("emergency-confirmation/{emergencyId}")]
        public async Task<IActionResult> GetEmergencyConfirmation(int emergencyId)
        {
            var result = await _emergencyContactService.GetEmergencyConfirmation(emergencyId);
            return Ok(result);
        }

        [HttpGet("list-emergency-confirmation/{elderlyId}")]
        public async Task<IActionResult> GetListEmergencyConfirmationByElderly(int elderlyId)
        {
            var result = await _emergencyContactService.GetListEmergencyConfirmationByElderly(elderlyId);
            return Ok(result);
        }

        [HttpPost("emergency-confirmation")]
        public async Task<IActionResult> CreateEmergencyConfirmation(int elderlyId)
        {
            var result = await _emergencyContactService.CreateEmergencyConfirmation(elderlyId);
            return Ok(result);
        }

        [HttpPost("emergency-information")]
        public async Task<IActionResult> CreateEmergencyInformation([FromForm] CreateEmergencyInformationRequest req)
        {
            var result = await _emergencyContactService.CreateEmergencyInformation(req);
            return Ok(result);
        }        
        
        [HttpPost("confirmation")]
        public async Task<IActionResult> ConfirmEmergency([FromQuery] int accountId, [FromQuery] int emergencyId)
        {
            var result = await _emergencyContactService.ConfirmEmergency(accountId, emergencyId);
            return Ok(result);
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
        
        [HttpPost("115-emergency-call")]
        public async Task<IActionResult> SoS115EmergencyCall()
        {
            var result = await _emergencyContactService.SoS115EmergencyCall();
            return Ok(result);
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

            await Task.Delay(5000);

            var cancelTask = WaitForUserCancellation(accountId, cts.Token);

            var completedTask = await Task.WhenAny(cancelTask, Task.Delay(5000)); 
            if (completedTask == cancelTask && cancelTask.Result)
            {
                cts.Cancel();
                CallStatuses[accountId] = "Cancelled";
                return Ok(new { Status = 1, Message = "Emergency call cancelled by user." });
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
                return Ok(new { Status = 1, Message = "Call cancelled successfully." });
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
                await Task.Delay(500, token);
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
}