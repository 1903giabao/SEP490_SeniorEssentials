using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request
{
    public class EvaluateHealthIndicatorRequest
    {
        public List<HealthIndicatorCheck> Indicators { get; set; }

    }

    public class HealthIndicatorCheck
    {
        public string Type { get; set; }
        public Decimal Value { get; set; }

        public string Time {  get; set; }
    }
}
