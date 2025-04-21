using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Response.Booking
{
    public class GetSubscriptionByUserResponse
    {
        public string SubscriptionName { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NumberOfMeetingLeft { get; set; }
    }
}
