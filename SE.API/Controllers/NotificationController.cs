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
    }
}
