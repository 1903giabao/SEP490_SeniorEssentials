using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Response.HealthIndicator
{
    public class GetAllHealthIndicatorReponse
    {
        public string Tabs { get; set; }
        public string Evaluation { get; set; }
        public string DateTime { get; set; }
        public string Indicator { get; set; }
        public string AverageIndicator { get; set; }
    }
}
