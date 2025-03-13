using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Response
{
    public class GetBloodPressureDetail
    {
        public string Tabs { get; set; }

        public string Average { get; set; }
        public string Evaluation { get; set; }
        public List<ChartBloodPressureModel> ChartDatabase { get; set; }
    }
    public class ChartBloodPressureModel
    {
        public string Type { get; set; }
        public string? Indicator { get; set; }
    }
}
