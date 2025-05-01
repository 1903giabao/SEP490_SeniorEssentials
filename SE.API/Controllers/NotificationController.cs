using Microsoft.AspNetCore.Mvc;
using SE.Service.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace SE.API.Controllers
{
    [Route("notification-management")]
    [ApiController]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost("push-notification")]
        public async Task<IActionResult> SendNotification(string token, string title, string body)
        {
            var result = await _notificationService.SendNotification(token, title, body);
            if (result != null)
            {
                return Ok(new { message = "Notification sent successfully" });
            }

            return BadRequest(new { message = "Failed to send notification" });
        }

        [HttpGet]

        public async Task<IActionResult> GetAllActivities(int accountId)
        {
            var result = await _notificationService.GetAllNotiInAccount(accountId);
            return Ok(result);
        }

        [HttpPut("update")]

        public async Task<IActionResult> UpdateStatusNotificaction( int notiId, string status)
        {
            var result = await _notificationService.UpdateStatusNotificaction(notiId, status);
            return Ok(result);
        }

        [HttpPost("send-notification-location")]
        public async Task<IActionResult> SendNotiToGetLocation([FromQuery] int familyMemberId, [FromQuery] int elderlyId)
        {
            var result = await _notificationService.SendNotiToGetLocation(familyMemberId, elderlyId);
            return Ok(result);
        }        
        
        [HttpGet("send-location")]
        public async Task<IActionResult> SendNotiLocation([FromQuery] int familyMemberId, [FromQuery] int elderlyId, [FromQuery] string? longitude, [FromQuery] string? latitude)
        {
            var result = await _notificationService.SendNotiLocation(familyMemberId, elderlyId, longitude, latitude);
            return Ok(result);
        }        
        
        [HttpGet("health-notification")]
        public async Task<IActionResult> SendNotToGetHealthIndicator([FromQuery] int accountId)
        {
            var result = await _notificationService.SendNotToGetHealthIndicator(accountId);
            return Ok(result);
        }
    }
}
