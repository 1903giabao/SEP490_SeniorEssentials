using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request.HealthIndicator
{
    public class CreateHeightRequest
    {
        public int AccountId { get; set; }
        public int ElderlyId { get; set; }

        public decimal? Height1 { get; set; }
        public string HeightSource { get; set; }
        public string CreatedBy { get; set; }

    }
}
