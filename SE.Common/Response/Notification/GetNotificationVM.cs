using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Response.Notification
{
    public class GetNotificationVM
    {
        public int NotificationId { get; set; }

        public int AccountId { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public string NotificationType { get; set; }

        public DateTime CreatedDate { get; set; }

        public string Status { get; set; }

        public string Data { get; set; }

        public int ElderlyId { get; set; }

        public string FullName { get; set; }

    }
}
