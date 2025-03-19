using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request.HealthIndicator
{
    public class CreateHeartRateRequest
    {
        public int AccountId { get; set; }
        public int ElderlyId { get; set; }

        public int? HeartRate1 { get; set; }
        public string HeartRateSource { get; set; }
    }
}
