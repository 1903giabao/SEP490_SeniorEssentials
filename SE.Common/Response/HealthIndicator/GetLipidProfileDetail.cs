using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Response.HealthIndicator
{
    public class GetLipidProfileDetail
    {

        public string Tabs { get; set; }
        public decimal? TotalCholesterolAverage { get; set; }

        public decimal? LdlcholesterolAverage { get; set; }

        public decimal? HdlcholesterolAverage { get; set; }

        public decimal? TriglyceridesAverage { get; set; }
        public double HighestPercent { get; set; }

        public double LowestPercent { get; set; }
        public double NormalPercent { get; set; }

        public List<CharLipidProfileModel> ChartDatabase { get; set; }
    }

    public class CharLipidProfileModel
    {
        public string Type { get; set; }
        public decimal? TotalCholesterol { get; set; }

        public decimal? Ldlcholesterol { get; set; }

        public decimal? Hdlcholesterol { get; set; }

        public decimal? Triglycerides { get; set; }
    }

}
