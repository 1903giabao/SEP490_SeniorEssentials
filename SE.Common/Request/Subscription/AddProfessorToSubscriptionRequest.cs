using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request.Subscription
{
    public class AddProfessorToSubscriptionRequest
    {
        public int ProfessorId { get; set; }
        public int ElderlyId { get; set; }
    }
}
