using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Response.HealthIndicator
{
    public class GetLiverEnzymesDetail
    {
        public string Tabs { get; set; }

        public decimal? AltAverage { get; set; }

        public decimal? AstAverage { get; set; }

        public decimal? AlpAverage { get; set; }

        public decimal? GgtAverage { get; set; }
        public double HighestPercent { get; set; }

        public double LowestPercent { get; set; }
        public double NormalPercent { get; set; }

        public List<CharLiverEnzymesModel> ChartDatabase { get; set; }


    }

    public class CharLiverEnzymesModel
    {
        public string Type { get; set; }
        public decimal? Alt { get; set; }

        public decimal? Ast { get; set; }

        public decimal? Alp { get; set; }

        public decimal? Ggt { get; set; }

    }
}
