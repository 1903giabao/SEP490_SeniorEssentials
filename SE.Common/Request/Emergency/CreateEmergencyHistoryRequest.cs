using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request.Emergency
{
    public class CreateEmergencyHistoryRequest
    {
        public int ElderlyId { get; set; }
        public int AccountId { get; set; }
        public bool IsConfirmed { get; set; }
    }
}
