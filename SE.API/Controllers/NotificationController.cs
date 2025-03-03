using Microsoft.AspNetCore.Mvc;
using SE.Service.Services;

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
    }
}
