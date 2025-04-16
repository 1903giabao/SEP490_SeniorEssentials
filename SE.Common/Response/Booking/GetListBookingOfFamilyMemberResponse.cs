using SE.Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Response.Booking
{
    public class GetListBookingOfFamilyMemberResponse
    {
        public int? BookingId { get; set; }
        public DateTime? BookingDate { get; set; }
        public double? Price { get; set; }
        public string? Note { get; set; }
        public UserDTO? Elderly { get; set; }
        public SubscriptionDTO? Subscription { get; set; }
    }
}
