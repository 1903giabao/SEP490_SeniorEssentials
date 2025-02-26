using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request.HealthIndicator
{
    public class CreateHeartRateRequest
    {
        public int ElderlyId { get; set; }
        public int? HeartRate { get; set; }
        public string HeartRateSource { get; set; }
    }
}
