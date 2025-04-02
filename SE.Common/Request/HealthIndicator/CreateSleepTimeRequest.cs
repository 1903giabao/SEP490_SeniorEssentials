using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request.HealthIndicator
{
    public class CreateSleepTimeRequest
    {
        public int AccountId { get; set; }
        public int ElderlyId { get; set; }
        public decimal? SleepTime1 { get; set; }
    }
}
