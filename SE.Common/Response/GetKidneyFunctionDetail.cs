using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Response
{
    public class GetKidneyFunctionDetail
    {
        public string Tabs { get; set; }
        public decimal? CreatinineAverage { get; set; }

        public decimal? BunAverage { get; set; }

        public decimal? EGfrAverage { get; set; }

        public double HighestPercent { get; set; }

        public double LowestPercent { get; set; }
        public double NormalPercent { get; set; }

        public List<CharKidneyFunctionModel> ChartDatabase { get; set; }
    }

    public class CharKidneyFunctionModel
    {
        public string Type { get; set; }
        public decimal? Creatinine { get; set; }

        public decimal? Bun { get; set; }

        public decimal? EGfr { get; set; }
    }
}
