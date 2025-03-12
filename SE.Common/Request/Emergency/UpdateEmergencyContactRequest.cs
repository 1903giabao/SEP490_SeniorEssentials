using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request.Emergency
{
    public class UpdateEmergencyContactRequest
    {
        public int ElderlyId { get; set; }
        public int AccountId { get; set; }
        public string ContactName { get; set; }
        public int Priority { get; set; }

    }
}
