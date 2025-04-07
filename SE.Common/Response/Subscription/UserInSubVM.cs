using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Response.Subscription
{
    public class UserInSubVM
    {
        public int SubscriptionId { get; set; }

        public string Name { get; set; }
        public decimal Fee { get; set; }
        public string Status { get; set; }
    
        public int FamilyMemberId { get; set; }

        public int ElderlyId { get; set; }

    }
}
