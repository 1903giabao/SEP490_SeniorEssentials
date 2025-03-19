using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request.HealthIndicator
{
    public class CreateWeightRequest
    {
        public int AccountId { get; set; }
        public int ElderlyId { get; set; }
        public decimal? Weight1 { get; set; }
        public string WeightSource { get; set; }
    }
}
