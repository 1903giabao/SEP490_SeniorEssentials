using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Response
{
    public class GetLiverEnzymesDetail
    {
        public string Tabs { get; set; }

        public decimal? AltAverage { get; set; }

        public decimal? AstAverage { get; set; }

        public decimal? AlpAverage { get; set; }

        public decimal? GgtAverage { get; set; }
        public string AltEvaluation { get; set; }

        public string AstEvaluation { get; set; }

        public string AlpEvaluation { get; set; }

        public string GgtEvaluation { get; set; }
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
