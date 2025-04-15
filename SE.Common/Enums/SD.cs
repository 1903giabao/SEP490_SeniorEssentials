using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Enums
{
    public class SD
    {
        private static SD instance;
        private SD()
        {
        }
        public static SD getInstance()
        {
            if (instance == null) instance = new SD();
            return instance;
        }

        public class GeneralStatus
        {
            public static string ACTIVE = "Active";
            public static string INACTIVE = "Inactive";
        }         
        
        public class ContentStatus
        {
            public static string ACTIVE = "Active";
            public static string INACTIVE = "Inactive";
            public static string ADMINDELETE = "AdminDelete";
        }  
        
        public class ProfessorAppointmentStatus
        {
            public static string NOTYET = "NotYet";
            public static string JOINED = "Joined";
            public static string CANCELLED = "Cancelled";

        }

        public class NotificationStatus
        {
            public static string SEND = "Chưa đọc";
            public static string SEEN = "Đã đọc";
        }

        public class UserLinkStatus
        {
            public static string PENDING = "Pending";
            public static string CANCELLED = "Cancelled";
            public static string ACCEPTED = "Accepted";
            public static string DELETED = "Deleted";
            public static string REJECTED = "Rejected";
        }       
        
        public class BookingStatus
        {
            public static string PENDING = "Pending";
            public static string CANCELLED = "Cancelled";
            public static string PAID = "Paid";
        }        
        
        public class EmergencyStatus
        {
            public static string CONFIRMED = "Đã xác nhận";
            public static string CANCELLED = "Đã hủy";
            public static string PENDING = "Chưa xác nhận";
        }        
        
        public class UserSubscriptionStatus
        {
            public static string AVAILABLE = "Đang khả dụng";
            public static string BOOKED = "Đã đặt lịch";
            public static string EXPIRED = "Đã hết hạn";
        }
    }
}
