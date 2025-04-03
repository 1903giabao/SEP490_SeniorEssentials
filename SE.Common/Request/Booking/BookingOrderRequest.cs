using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request.Booking
{
    public class BookingOrderRequest
    {
        public int AccountId { get; set; }
        public int ElderlyId { get; set; }
        public int SubscriptionId { get; set; }
    }
}
