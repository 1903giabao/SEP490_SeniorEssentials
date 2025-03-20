using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Response.HealthIndicator
{
    public class GetBloodGlucoseResponse
    {
        public string Tabs { get; set; }

        public double Highest { get; set; }

        public double Lowest { get; set; }
        public double Average { get; set; }

        public double HighestPercent { get; set; }

        public double LowestPercent { get; set; }
        public double NormalPercent { get; set; }
        public List<ChartBloodGlucoseModel> ChartDatabase { get; set; }
    }

    public class ChartBloodGlucoseModel
    {
        public string Type { get; set; }
        public double? Indicator { get; set; }
    }
}
