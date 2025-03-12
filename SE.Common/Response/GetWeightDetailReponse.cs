using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Response
{
    public class GetWeightDetailReponse
    {
        public string Tabs {  get; set; }

        public double BMI { get; set; }
        public List<ChartDatModel> ChartDatabase { get; set; }
    }

    public class ChartDatModel
    {
        public string Type { get; set; }
        public double? Indicator {  get; set; }
    }

}

