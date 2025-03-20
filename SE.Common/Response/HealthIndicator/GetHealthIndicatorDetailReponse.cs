using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Response.HealthIndicator
{
    public class GetHealthIndicatorDetailReponse
    {
        public string Tabs { get; set; }

        public double Average { get; set; }
        public string Evaluation { get; set; }
        public List<ChartDataModel> ChartDatabase { get; set; }
    }

    public class ChartDataModel
    {
        public string Type { get; set; }
        public double? Indicator { get; set; }
    }

}

